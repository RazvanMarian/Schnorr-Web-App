#include "Signature.cpp"
#include "Generate.cpp"
#include "IO.cpp"
#include "Certificates.cpp"

extern "C"
{

    int Generate(const char *privateFilename, const char *publicFilename)
    {
        EC_KEY *key;
        int res = Gen(&key);
        if (res != 0)
        {
            std::cout << "Eroare la generarea cheii!" << std::endl;
            return res;
        }

        res = Write_Schnorr_Private_Key(key, privateFilename);
        if (res != 0)
        {
            std::cout << "Eroare la scrierea cheii in fisier!" << std::endl;
            return res;
        }

        res = Write_Schnorr_Public_Key(key, publicFilename);
        if (res != 0)
        {
            std::cout << "Eroare la scrierea cheii in fisier" << std::endl;
            return res;
        }

        return res;
    }

    void test_sign(const char *hash)
    {
        int nr_semnatari = 1000;
        EC_KEY *keys[nr_semnatari];
        int res;
        for (int i = 0; i < nr_semnatari; i++)
        {

            res = Gen(&(keys[i]));
            if (res != 0)
            {
                std::cout << "Eroare la generarea cheii!" << std::endl;
                return;
            }
        }

        schnorr_signature sig;
        res = Schnorr_Multiple_Sign(keys, nr_semnatari, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la crearea semnaturii" << std::endl;
            return;
        }

        res = Verify_Multiple_Sign(keys, nr_semnatari, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la verificarea semnaturii" << std::endl;
            return;
        }
        std::cout << "Semnare si verificare fisier cu " << nr_semnatari << " numar semnatari ok!" << std::endl;

        return;
    }

    void Sign_Document_Test(const char *hash, const char *privateFilename, const char *publicFilename)
    {
        EC_KEY *sign_key;
        EC_KEY *verify_key;
        schnorr_signature sig;

        int res = Read_Schnorr_Private_key(&sign_key, privateFilename);
        if (res != 0)
        {
            std::cout << "Eroare la citirea cheii private!" << std::endl;
            return;
        }

        res = Schnorr_Sign(sign_key, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la semnare!" << std::endl;
            return;
        }

        res = Read_Schnorr_Public_Key(&verify_key, publicFilename);
        if (res != 0)
        {
            std::cout << "Eroare la citirea cheii private" << std::endl;
            return;
        }

        res = Verify_Sign(verify_key, hash, SHA256_DIGEST_LENGTH, sig);

        if (res != 0)
        {
            std::cout << "Eroare la verificare" << std::endl;
            return;
        }

        std::cout << "Semnare verificare ok cu cheia de la calea!" << privateFilename << std::endl;
    }

    int Generate_Certificate(const char *privateFilename, const char *publicFilename)
    {
        EC_KEY *private_key;
        EC_KEY *public_key;

        int res = Read_Schnorr_Private_key(&private_key, privateFilename);
        if (res != 0)
        {
            std::cout << "Eroare la citirea cheii private!" << std::endl;
            return 1;
        }

        res = Read_Schnorr_Public_Key(&public_key, publicFilename);
        if (res != 0)
        {
            std::cout << "Eroare la citirea cheii private!" << std::endl;
            return 1;
        }

        res = Create_Certificate(private_key, public_key);
        if (res != 0)
        {
            std::cout << "Eroare la creare certificat!" << std::endl;
            return 1;
        }

        return 0;
    }
}
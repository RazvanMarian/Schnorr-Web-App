#include "Signature.cpp"
#include "Generate.cpp"
#include "IO.cpp"

extern "C"
{

    int Generate(const char *privateFilename, const char *publicFilename)
    {
        EC_KEY *key;
        int res = Gen(&key);
        if (res != 0)
        {
            std::cout << "Eroare la generarea cheii" << std::endl;
            return res;
        }

        res = Write_Schnorr_Private_Key(key, privateFilename);
        if (res != 0)
        {
            std::cout << "Eroare la scrierea cheii in fisier" << std::endl;
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
                std::cout << "Eroare la generarea cheii" << std::endl;
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
}
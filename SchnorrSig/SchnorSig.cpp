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
        EC_KEY *keys[3];
        int res;
        for (int i = 0; i < 3; i++)
        {

            res = Gen(&(keys[i]));
            if (res != 0)
            {
                std::cout << "Eroare la generarea cheii" << std::endl;
                return;
            }
        }

        schnorr_signature sig;
        res = Schnorr_Multiple_Sign(keys, 3, hash, SHA256_DIGEST_LENGTH, sig);

        res = Verify_Multiple_Sign(keys, 3, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la verificarea semnaturii" << std::endl;
        }
        std::cout << "Semnare si verificare fisier cu 3 semnatari ok!" << std::endl;

        return;
    }
}
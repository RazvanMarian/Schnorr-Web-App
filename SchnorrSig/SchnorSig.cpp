#include "Signature.cpp"
#include "Generate.cpp"
#include "IO.cpp"

extern "C"
{
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
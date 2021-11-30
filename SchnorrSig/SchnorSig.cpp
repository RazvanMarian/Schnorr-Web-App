#include "Signature.cpp"
#include "Generate.cpp"
#include "IO.cpp"

extern "C"
{

    void test_sign(const char *hash)
    {
        // std::cout << "Hash:" << hash << std::endl;

        // EC_KEY *key;
        // int res = Gen(&key);
        // if (res != 0)
        // {
        //     std::cout << "Eroare la generarea cheii" << std::endl;
        //     return;
        // }

        // schnorr_signature sig;
        // res = Schnorr_Sign(key, hash, SHA256_DIGEST_LENGTH, sig);
        // if (res != 0)
        // {
        //     std::cout << "Eroare la generarea crearea semnaturii" << std::endl;
        //     return;
        // }
        // BN_print_fp(stdout, sig.s);
        // std::cout << std::endl;
        // res = Verify_Sign(key, hash, SHA256_DIGEST_LENGTH, sig);
        // if (res != 0)
        // {
        //     std::cout << "Eroare la verificare semnatura";
        //     return;
        // }

        // std::cout << "Generare, Semnare, Verificare OK :) phew" << std::endl;

        EC_KEY *keys[2];
        int res = Gen(&(keys[0]));
        if (res != 0)
        {
            std::cout << "Eroare la generarea cheii" << std::endl;
            return;
        }

        res = Gen(&(keys[1]));
        if (res != 0)
        {
            std::cout << "Eroare la generarea cheii" << std::endl;
            return;
        }

        schnorr_signature sig;
        res = Schnorr_Multiple_Sign(keys, 2, hash, SHA256_DIGEST_LENGTH, sig);
        std::cout << "s: ";
        BN_print_fp(stdout, sig.s);
    }
}
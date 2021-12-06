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

        // PEM_Write_SchnorrPrivateKEY(keys[0], "/home/razvan/keys/key.prv", NULL, NULL, 0, NULL, NULL);
        // PEM_Write_SchnorrPUBKEY(keys[0], "/home/razvan/keys/key.pub");
        write_schnorr_private_key(keys[0], "/home/razvan/keys/key.prv");

        schnorr_signature sig;
        res = Schnorr_Multiple_Sign(keys, 3, hash, SHA256_DIGEST_LENGTH, sig);
        printf("s:");
        BN_print_fp(stdout, sig.s);
        std::cout << std::endl;

        res = Verify_Multiple_Sign(keys, 3, hash, SHA256_DIGEST_LENGTH, sig);
        if (res != 0)
        {
            std::cout << "Eroare la verificarea semnaturii" << std::endl;
        }
        std::cout << "Semnare si verificare fisier cu 3 semnatari ok!";
        std::cout << std::endl;

        return;
    }
}
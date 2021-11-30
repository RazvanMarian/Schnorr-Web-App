#include "Utils.cpp"

int PEM_Write_SchnorrPUBKEY(EC_KEY *key, const char *filename)
{
    if (key == NULL)
    {
        std::cout << "Key is NULL!" << std::endl;
        return -1;
    }

    FILE *fout = fopen(filename, "wb");
    if (fout == NULL)
    {
        std::cout << "The file does not exist!" << std::endl;
        return -1;
    }

    if (!PEM_write_EC_PUBKEY(fout, key))
    {
        std::cout << "Error at writing into the file!" << std::endl;
        return -1;
    }

    return 0;
}

int PEM_Write_SchnorrPrivateKEY(EC_KEY *key, const char *filename, const EVP_CIPHER *cipher, unsigned char *kstr, int klen, pem_password_cb *cb, void *u)
{
    if (key == NULL)
    {
        std::cout << "Key is NULL!" << std::endl;
        return -1;
    }

    FILE *fout = fopen(filename, "wb");
    if (fout == NULL)
    {
        std::cout << "The file does not exist!" << std::endl;
        return -1;
    }

    if (!PEM_write_ECPrivateKey(fout, key, cipher, kstr, klen, cb, u))
    {
        std::cout << "Error at writing into the file!" << std::endl;
        return -1;
    }
    return 0;
}

int PEM_Read_SchnorrPUBKEY(EC_KEY **key, const char *filename, pem_password_cb *cb, void *u)
{
    *key = EC_KEY_new();

    FILE *fin = fopen(filename, "rb");
    if (fin == NULL)
    {
        std::cout << "The file does not exist!" << std::endl;
        return -1;
    }

    PEM_read_EC_PUBKEY(fin, key, cb, u);
    if (*key == NULL)
    {
        std::cout << "Error reading the key!" << std::endl;
        return -1;
    }

    return 0;
}

int PEM_Read_SchnorrPrivateKEY(EC_KEY **key, const char *filename, pem_password_cb *cb, void *u)
{
    *key = EC_KEY_new();

    FILE *fin = fopen(filename, "rb");
    if (fin == NULL)
    {
        std::cout << "The file does not exist!" << std::endl;
        return -1;
    }

    PEM_read_ECPrivateKey(fin, key, cb, u);
    if (*key == NULL)
    {
        std::cout << "Error reading the key!" << std::endl;
        return -1;
    }

    return 0;
}
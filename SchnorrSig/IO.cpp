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
    printf("Written succesfully\n");
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

int write_schnorr_private_key(EC_KEY *key, const char *filename)
{
    BIO *output = NULL;
    EVP_PKEY *pkey = NULL;

    OpenSSL_add_all_algorithms();
    ERR_load_BIO_strings();
    ERR_load_crypto_strings();

    FILE *fout = fopen(filename, "wb");
    if (fout == NULL)
    {
        std::cout << "The file does not exist!" << std::endl;
        return -1;
    }

    output = BIO_new_file(filename, "wb");
    BIO *outputstd = BIO_new(BIO_s_file());
    outputstd = BIO_new_fp(stdout, BIO_NOCLOSE);

    /* -------------------------------------------------------- *
     * Converting the EC key into a PKEY structure let us       *
     * handle the key just like any other key pair.             *
     * ---------------------------------------------------------*/

    pkey = EVP_PKEY_new();
    if (!EVP_PKEY_assign_EC_KEY(pkey, key))
        BIO_printf(output, "Error assigning ECC key to EVP_PKEY structure.");

    /*Now we show how to extract EC - specifics from the key * * -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -- -*/
    key = EVP_PKEY_get1_EC_KEY(pkey);
    const EC_GROUP *ecgrp = EC_KEY_get0_group(key);

    /* ---------------------------------------------------------- *
     * Here we print the key length, and extract the curve type.  *
     * ---------------------------------------------------------- */
    BIO_printf(outputstd, "ECC Key size: %d bit\n", EVP_PKEY_bits(pkey));
    BIO_printf(outputstd, "ECC Key type: %s\n", OBJ_nid2sn(EC_GROUP_get_curve_name(ecgrp)));

    /* ---------------------------------------------------------- *
     * Here we print the private/public key data in PEM format.   *
     * ---------------------------------------------------------- */
    if (!PEM_write_bio_PrivateKey(output, pkey, NULL, NULL, 0, 0, NULL))
        BIO_printf(output, "Error writing private key data in PEM format");

    if (!PEM_write_bio_PUBKEY(outputstd, pkey))
        BIO_printf(outputstd, "Error writing public key data in PEM format");

    /* ---------------------------------------------------------- *
     * Free up all structures                                     *
     * ---------------------------------------------------------- */
    EVP_PKEY_free(pkey);
    // EC_KEY_free(myecc);
    BIO_free_all(output);
    return 0;
}
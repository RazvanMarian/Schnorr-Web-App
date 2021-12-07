#include "Utils.cpp"

int Write_Schnorr_Private_Key(EC_KEY *key, const char *filename)
{
    BIO *output = NULL;

    OpenSSL_add_all_algorithms();
    ERR_load_BIO_strings();
    ERR_load_crypto_strings();

    output = BIO_new_file(filename, "wb");

    if (!PEM_write_bio_ECPrivateKey(output, key, NULL, NULL, 0, 0, NULL))
        BIO_printf(output, "Error writing private key in PEM format");

    // Free
    BIO_free_all(output);
    return 0;
}

int Write_Schnorr_Public_Key(EC_KEY *key, const char *filename)
{
    BIO *output = NULL;

    OpenSSL_add_all_algorithms();
    ERR_load_BIO_strings();
    ERR_load_crypto_strings();

    output = BIO_new_file(filename, "wb");

    if (!PEM_write_bio_EC_PUBKEY(output, key))
        BIO_printf(output, "Error writing public key in PEM format");

    // Free
    BIO_free_all(output);
    return 0;
}

int Read_Schnorr_Private_key(EC_KEY **key, const char *filename)
{
    BIO *output = NULL;
    *key = EC_KEY_new();

    OpenSSL_add_all_algorithms();
    ERR_load_BIO_strings();
    ERR_load_crypto_strings();

    output = BIO_new_file(filename, "rb");

    if (!PEM_read_bio_ECPrivateKey(output, key, 0, NULL))
    {
        BIO_printf(output, "Error reading private key in PEM format");
        return 1;
    }

    // Free
    BIO_free_all(output);
    return 0;
}

int Read_Schnorr_Public_Key(EC_KEY **key, const char *filename)
{
    BIO *output = NULL;
    *key = EC_KEY_new();

    OpenSSL_add_all_algorithms();
    ERR_load_BIO_strings();
    ERR_load_crypto_strings();

    output = BIO_new_file(filename, "rb");

    if (!PEM_read_bio_EC_PUBKEY(output, key, 0, NULL))
    {
        BIO_printf(output, "Error reading public key in PEM format");
        return 1;
    }

    // Free
    BIO_free_all(output);

    return 0;
}
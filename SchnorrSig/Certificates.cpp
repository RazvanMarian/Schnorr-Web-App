#include "Utils.cpp"
#include <openssl/x509.h>

int Create_Certificate(EC_KEY *private_key, EC_KEY *public_key)
{
    OpenSSL_add_all_algorithms();
    ERR_load_BIO_strings();
    ERR_load_crypto_strings();

    EVP_PKEY *pkey = EVP_PKEY_new();
    EVP_PKEY *pkey2 = EVP_PKEY_new();

    if (!EVP_PKEY_assign_EC_KEY(pkey, public_key))
    {
        std::cout << "Erroare asignare cheie ECC in structura EVP_PKEY." << std::endl;
        return 1;
    }

    if (!EVP_PKEY_assign_EC_KEY(pkey2, private_key))
    {
        std::cout << "Erroare asignare cheie ECC in structura  priv EVP_PKEY." << std::endl;
        return 1;
    }

    X509 *x509;
    x509 = X509_new();

    ASN1_INTEGER_set(X509_get_serialNumber(x509), 1);

    X509_gmtime_adj(X509_get_notBefore(x509), 0);
    X509_gmtime_adj(X509_get_notAfter(x509), 31536000L);

    int ret = X509_set_pubkey(x509, pkey);
    if (ret == 0)
    {
        std::cout << "Eroare la setarea cheii publice!" << std::endl;
        return -1;
    }

    X509_NAME *name;
    name = X509_get_subject_name(x509);

    X509_NAME_add_entry_by_txt(name, "C", MBSTRING_ASC,
                               (unsigned char *)"RO", -1, -1, 0);
    X509_NAME_add_entry_by_txt(name, "O", MBSTRING_ASC,
                               (unsigned char *)"Schnorr Sign Inc.", -1, -1, 0);
    X509_NAME_add_entry_by_txt(name, "CN", MBSTRING_ASC,
                               (unsigned char *)"localhost", -1, -1, 0);

    X509_set_issuer_name(x509, name);

    ret = X509_sign(x509, pkey2, EVP_sha256());
    if (ret == 0)
    {
        std::cout << "Eroare la semnarea certificatului" << std::endl;
        return -1;
    }

    BIO *output = BIO_new_file((const char *)"/home/razvan/certificates/cert.pem", "wb");

    ret = PEM_write_bio_X509(output, x509);
    if (ret == 0)
    {
        std::cout << "Eroare la scriere in fisier" << std::endl;
        return -1;
    }
    BIO_free_all(output);

    return 0;
}
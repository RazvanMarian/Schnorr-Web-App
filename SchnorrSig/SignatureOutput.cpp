#include <openssl/asn1.h>
#include <openssl/asn1t.h>
#include <openssl/schnorr.h>
#include <openssl/x509.h>
#include <openssl/objects.h>

typedef struct SIG
{
    ASN1_INTEGER *r;
    ASN1_INTEGER *s;
} SIG;

typedef struct Content
{
    ASN1_OBJECT *digest_identifier;
    SIG *signature;
    ASN1_PRINTABLESTRING *cert;
} Content;

ASN1_SEQUENCE(SIG) = {
    ASN1_SIMPLE(SIG, r, ASN1_INTEGER),
    ASN1_SIMPLE(SIG, s, ASN1_INTEGER)} ASN1_SEQUENCE_END(SIG);

DECLARE_ASN1_FUNCTIONS(SIG);
IMPLEMENT_ASN1_FUNCTIONS(SIG);

ASN1_SEQUENCE(Content) = {
    ASN1_SIMPLE(Content, digest_identifier, ASN1_OBJECT),
    ASN1_SIMPLE(Content, signature, SIG),
    ASN1_SIMPLE(Content, cert, ASN1_PRINTABLESTRING)} ASN1_SEQUENCE_END(Content);

DECLARE_ASN1_FUNCTIONS(Content);
IMPLEMENT_ASN1_FUNCTIONS(Content);

int writeSignature(SCHNORR_SIG *signature, X509 *cert)
{

    if (cert == NULL || signature == NULL)
    {
        return -1;
    }
    const char OBJ_Id_Sha256[] = "2.16.840.1.101.3.4.2.1";

    auto sig = SIG_new();

    sig->r = BN_to_ASN1_INTEGER(SCHNORR_SIG_get_r(signature), NULL);
    sig->s = BN_to_ASN1_INTEGER(SCHNORR_SIG_get_s(signature), NULL);

    if (sig->r == NULL || sig->s == NULL)
    {
        return -1;
    }

    auto content = Content_new();
    content->digest_identifier = OBJ_txt2obj(OBJ_Id_Sha256, 0);

    int len = i2d_X509(cert, NULL);
    if (len <= 0)
        return -1;
    unsigned char *buffer = (unsigned char *)OPENSSL_malloc(len);
    unsigned char *aux;
    aux = buffer;
    i2d_X509(cert, &aux);

    content->cert = ASN1_PRINTABLESTRING_new();
    ASN1_STRING_set(content->cert, buffer, len);

    content->signature = sig;

    len = i2d_Content(content, NULL);
    if (len <= 0)
        return -1;
    unsigned char *buf = (unsigned char *)OPENSSL_malloc(len);
    unsigned char *aux2 = buf;
    i2d_Content(content, &aux2);

    for (int i = 0; i < len; i++)
    {
        printf("%x", buf[i]);
    }
    printf("\n");

    FILE *fout = fopen("/home/razvan/temp_files/data.bin", "wb");
    fwrite(buf, len, 1, fout);
    fclose(fout);

    return 0;
}
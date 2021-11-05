#include <stdio.h>
#include <string.h>
#include <iostream>
#include <string>
#include <openssl/ec.h>
#include <openssl/evp.h>
#include <openssl/bn.h>
#include <openssl/sha.h>
#include <openssl/pem.h>


//Base point coordinates
#define xG "79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798"
#define yG "483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8"

#define BASE_ERROR 100
#define MEMORY_ERROR 101
#define GROUP_ERROR 102
#define POINT_ERROR 104
#define POINT_CONVERSION_ERROR 105
#define CALCULATION_ERROR 106
#define GENERATE_ERROR 107
#define ORDER_ERROR 108
#define SIGNATURE_ERROR 110
#define KEY_ERROR 112


extern "C"
{

    typedef struct schnorr_signature {
        EC_POINT* R;
        BIGNUM* s;
    };

    void Hello()
    {
        
        BIGNUM* a = BN_new();
        BN_rand(a, 1024, BN_RAND_BOTTOM_ANY, BN_RAND_TOP_ANY);
        char* to=(char*)malloc(BN_num_bytes(a));
        to=BN_bn2hex(a);
        std::string str((char*)to);
        std::cout<<str;
        std::cout << "Ceva" << std::endl;
    }

    int Gen(EC_KEY** key) {
        EC_POINT* Q, * G;
        BIGNUM* a, * x, * y, * order;
        EC_GROUP* group;

        try {
            group = EC_GROUP_new_by_curve_name(NID_secp256k1);
            if (group == nullptr)
            {
                std::cout << "The curve group does not exist!" << std::endl;
                throw GROUP_ERROR;
            }


            //BASE POINT G
            G = EC_POINT_new(group);
            if (G == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            x = BN_new();
            y = BN_new();
            if (x == nullptr || y == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }
            BN_hex2bn(&x, xG);
            BN_hex2bn(&y, yG);

            if (!EC_POINT_set_affine_coordinates(group, G, x, y, nullptr)) {
                std::cout << "The point does not belong to the curve!" << std::endl;
                throw POINT_ERROR;
            }


            if (!EC_POINT_is_on_curve(group, G, nullptr)) {
                std::cout << "The point does not belong to the curve!" << std::endl;
                throw POINT_ERROR;
            }

            //*************************************************************************************************************
            order = BN_new();
            if (EC_GROUP_get_order(group, order, nullptr) == 0)
            {
                std::cout << "Order error" << std::endl;
                throw ORDER_ERROR;
            }

            a = BN_new();
            if (a == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }

            Q = EC_POINT_new(group);
            if (Q == nullptr)
            {
                std::cout << "Memory error" << std::endl;
                throw MEMORY_ERROR;
            }

            //Generate random integer a between [1,order]
            BN_rand_range(a, order);
            //Calculate Q = a * P
            EC_POINT_mul(group, Q, nullptr, G, a, nullptr);

            if (!EC_POINT_is_on_curve(group, Q, nullptr))
            {
                std::cout << "The point does not belong to the curve!" << std::endl;
                throw POINT_ERROR;
            }


            *key = EC_KEY_new();
            if (!EC_KEY_set_group(*key, group)) {
                std::cout << "Key generation error!" << std::endl;
                throw KEY_ERROR;
            }

            if (!EC_KEY_set_private_key(*key, a)) {
                std::cout << "Key generation error!" << std::endl;
                throw KEY_ERROR;
            }

            if (!EC_KEY_set_public_key(*key, Q)) {
                std::cout << "Key generation error!" << std::endl;
                throw KEY_ERROR;
            }

        }
        catch (int err)
        {
            std::cout << "ERROR: " << err << std::endl;
            return err;
        }

        EC_POINT_free(G);
        EC_POINT_free(Q);
        EC_GROUP_free(group);
        BN_free(a);
        BN_free(x);
        BN_free(y);
        BN_free(order);


        return 0;
    }

    int Sign(EC_KEY* key, schnorr_signature& sig, const unsigned char* message, int len)
{
    const BIGNUM* a;
    EC_POINT* G;
    BIGNUM* x, * y, * k, * order, * e;
    BN_CTX* ctx;
    EC_GROUP* group;

    try {

        group = EC_GROUP_new_by_curve_name(NID_secp256k1);
        if (group == nullptr)
        {
            std::cout << "The curve group does not exist!" << std::endl;
            throw GROUP_ERROR;
        }

        sig.R = EC_POINT_new(group);
        if (sig.R == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        order = BN_new();
        if (order == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        if (EC_GROUP_get_order(group, order, nullptr) == 0)
        {
            std::cout << "Order error" << std::endl;
            throw ORDER_ERROR;
        }
        k = BN_new();
        if (k == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }

        //Generate random integer k between [1,order]
        BN_rand_range(k, order);


        //BASE POINT G
        G = EC_POINT_new(group);
        if (G == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        x = BN_new();
        y = BN_new();
        if (x == nullptr || y == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        BN_hex2bn(&x, xG);
        BN_hex2bn(&y, yG);

        if (!EC_POINT_set_affine_coordinates(group, G, x, y, nullptr)) {
            std::cout << "The point does not belong to the curve!" << std::endl;
            throw POINT_ERROR;
        }


        //Calculate R = k*G
        EC_POINT_mul(group, sig.R, NULL, G, k, NULL);

        if (!EC_POINT_is_on_curve(group, sig.R, NULL))
        {
            std::cout << "The point does not belong to the curve!" << std::endl;
            throw POINT_ERROR;
        }

        unsigned char* r = new unsigned char[65];
        if (r == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        //Conversion from point to buffer for signing
        int size = EC_POINT_point2buf(group, sig.R, POINT_CONVERSION_HYBRID, &r, nullptr);

        if (size == 0)
        {
            std::cout << "Point conversion error" << std::endl;
            throw POINT_CONVERSION_ERROR;
        }

        unsigned char* toHash = new unsigned char[len + size];
        unsigned char* hash = new unsigned char[32];
        memcpy(toHash, message, len);
        memcpy(toHash + len, r, size);

        //Calculate H(M || R)
        SHA256(toHash, len + size, hash);

        e = BN_new();
        if (e == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        //Calculate e=H(M || R)
        BN_bin2bn(hash, 32, e);

        ctx = BN_CTX_new();
        sig.s = BN_new();
        if (sig.s == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }


        //Calculate s=(k+a*e) mod order
        a = BN_new();
        a = EC_KEY_get0_private_key(key);
        BN_mod_mul(e, e, a, order, ctx);
        BN_mod_add(sig.s, k, e, order, ctx);
    }
    catch (int err)
    {
        std::cout << "ERROR: " << err << std::endl;
        return err;
    }

    EC_POINT_free(G);
    BN_free(order);
    BN_free(e);
    BN_free(x);
    BN_free(y);
    BN_free(k);
    EC_GROUP_free(group);

    return 0;
}

    int Verify(EC_KEY* key, schnorr_signature& sig, const unsigned char* message, int len)
{
    const EC_POINT* Q;
    EC_POINT* G, * eByQ, * sByG;
    BIGNUM* e, * x, * y;
    EC_GROUP* group;

    try {
        group = EC_GROUP_new_by_curve_name(NID_secp256k1);
        if (group == nullptr)
        {
            std::cout << "The curve group does not exist!" << std::endl;
            throw GROUP_ERROR;
        }

        unsigned char* r = new unsigned char[65];
        if (r == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }

        int size = EC_POINT_point2buf(group, sig.R, POINT_CONVERSION_HYBRID, &r, nullptr);
        if (size == 0)
        {
            std::cout << "Point conversion error" << std::endl;
            throw POINT_CONVERSION_ERROR;
        }

        unsigned char* toHash = new unsigned char[len + size];
        if (toHash == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }

        unsigned char* hash = new unsigned char[32];
        if (hash == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        memcpy(toHash, message, len);
        memcpy(toHash + len, r, size);

        //Calculate H(M || R)
        SHA256(toHash, len + size, hash);

        e = BN_new();
        if (e == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        //Calculate e=H(M || R)
        BN_bin2bn(hash, 32, e);

        eByQ = EC_POINT_new(group);
        if (eByQ == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }

        sByG = EC_POINT_new(group);
        if (sByG == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }


        //Verification
        //R + e*Q = s*G
        Q = EC_POINT_new(group);
        if (Q == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        Q = EC_KEY_get0_public_key(key);
        //e*Q
        if (!EC_POINT_mul(group, eByQ, NULL, Q, e, NULL)) {
            std::cout << "Calculation error" << std::endl;
            throw CALCULATION_ERROR;
        }
        //R+e*Q
        if (!EC_POINT_add(group, eByQ, sig.R, eByQ, NULL)) {
            std::cout << "Calculation error" << std::endl;
            throw CALCULATION_ERROR;
        }

        //s*G
        //BASE POINT G
        G = EC_POINT_new(group);
        if (G == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        x = BN_new();
        y = BN_new();
        if (x == nullptr || y == nullptr)
        {
            std::cout << "Memory error" << std::endl;
            throw MEMORY_ERROR;
        }
        BN_hex2bn(&x, xG);
        BN_hex2bn(&y, yG);

        if (!EC_POINT_set_affine_coordinates(group, G, x, y, nullptr)) {
            std::cout << "The point does not belong to the curve!" << std::endl;
            throw POINT_ERROR;
        }

        if (!EC_POINT_mul(group, sByG, NULL, G, sig.s, NULL)) {
            std::cout << "Calculation error" << std::endl;
            throw CALCULATION_ERROR;
        }

        //Final comparison
        int end = EC_POINT_cmp(group, eByQ, sByG, NULL);
        if (end != 0)
        {
            std::cout << "The signature is not ok!" << std::endl;
            return SIGNATURE_ERROR;
        }
    }
    catch (int err)
    {
        std::cout << "ERROR: " << err << std::endl;
        return err;
    }
    EC_POINT_free(eByQ);
    EC_POINT_free(sByG);
    EC_POINT_free(G);
    BN_free(e);
    BN_free(x);
    BN_free(y);
    EC_GROUP_free(group);
    return 0;
}

    int PEM_Write_SchnorrPUBKEY(EC_KEY* key, const char* filename)
{
    if (key == NULL)
    {
        std::cout << "Key is NULL!" << std::endl;
        return -1;
    }

    FILE* fout = fopen(filename, "wb");
    if (fout == NULL)
    {
        std::cout << "The file does not exist!" << std::endl;
        return -1;
    }

    if (!PEM_write_EC_PUBKEY(fout, key)) {
        std::cout << "Error at writing into the file!" << std::endl;
        return -1;
    }

    return 0;
}

    int PEM_Write_SchnorrPrivateKEY(EC_KEY* key, const char* filename, const EVP_CIPHER* cipher, unsigned char* kstr, int klen, pem_password_cb* cb, void* u)
{
    if (key == NULL)
    {
        std::cout << "Key is NULL!" << std::endl;
        return -1;
    }

    FILE* fout = fopen(filename, "wb");
    if (fout == NULL)
    {
        std::cout << "The file does not exist!" << std::endl;
        return -1;
    }

    if (!PEM_write_ECPrivateKey(fout, key, cipher, kstr, klen, cb, u)) {
        std::cout << "Error at writing into the file!" << std::endl;
        return -1;
    }
}

    int PEM_Read_SchnorrPUBKEY(EC_KEY** key, const char* filename, pem_password_cb* cb, void* u)
{
    *key = EC_KEY_new();

    FILE* fin = fopen(filename, "rb");
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

    int PEM_Read_SchnorrPrivateKEY(EC_KEY** key, const char* filename, pem_password_cb* cb, void* u)
    {
        *key = EC_KEY_new();

        FILE* fin = fopen(filename, "rb");
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
}
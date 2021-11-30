#include "Utils.cpp"

int Gen(EC_KEY **key)
{
    EC_POINT *Q, *G;
    BIGNUM *a, *x, *y, *order;
    EC_GROUP *group;

    try
    {
        group = EC_GROUP_new_by_curve_name(NID_secp256k1);
        if (group == nullptr)
        {
            std::cout << "The curve group does not exist!" << std::endl;
            throw GROUP_ERROR;
        }

        // BASE POINT G
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

        if (!EC_POINT_set_affine_coordinates(group, G, x, y, nullptr))
        {
            std::cout << "The point does not belong to the curve!" << std::endl;
            throw POINT_ERROR;
        }

        if (!EC_POINT_is_on_curve(group, G, nullptr))
        {
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

        // Generate random integer a between [1,order]
        BN_rand_range(a, order);
        // Calculate Q = a * P
        EC_POINT_mul(group, Q, nullptr, G, a, nullptr);

        if (!EC_POINT_is_on_curve(group, Q, nullptr))
        {
            std::cout << "The point does not belong to the curve!" << std::endl;
            throw POINT_ERROR;
        }

        *key = EC_KEY_new();
        if (!EC_KEY_set_group(*key, group))
        {
            std::cout << "Key generation error!" << std::endl;
            throw KEY_ERROR;
        }

        if (!EC_KEY_set_private_key(*key, a))
        {
            std::cout << "Key generation error!" << std::endl;
            throw KEY_ERROR;
        }

        if (!EC_KEY_set_public_key(*key, Q))
        {
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

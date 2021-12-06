#include <stdio.h>
#include <string.h>
#include <iostream>
#include <string>
#include <openssl/ec.h>
#include <openssl/evp.h>
#include <openssl/bn.h>
#include <openssl/sha.h>
#include <openssl/pem.h>
#include <openssl/err.h>

// Base point coordinates
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
#define VERIFICATION_ERROR 114

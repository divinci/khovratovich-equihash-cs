# Khovratovich's Equihash in C\# #

A C# translation of **Dmitry Khovratovich's** original C++11 **Equihash** reference implementation.

See the original version over at his GitHub page: (https://github.com/khovratovich/equihash)

##I have striven not to introduce any optimisations, behaviour changes or fixes into the translated implementation.##

BUT with the only exception being that I have changed the way the memory structure is represented as follows:

The original C++ 'Tuple' class:
            
class Tuple {
public:
    std::vector<uint32_t> blocks;
    uint32_t reference;
};

~ has a uin32 list "blocks" and another uint32 "reference".
                
This translation will not reference a Tuple class, but instead it will treat all Tuple's
as uint32 arrays with an additional element at the end of the array for storing the 'reference' block.
The reference block's index will be stored in a variable REFERENCE_BLOCK_INDEX for ease of reading.




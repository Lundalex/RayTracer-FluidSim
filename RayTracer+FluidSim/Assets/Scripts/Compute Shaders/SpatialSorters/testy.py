Offsets_2x2x2 = [
    "(0,0,0)",  # Bit 0
    "(1,0,0)",  # Bit 1
    "(1,0,1)",  # Bit 2
    "(0,0,1)",  # Bit 3
    "(0,1,0)",  # Bit 4
    "(1,1,0)",  # Bit 5
    "(1,1,1)",  # Bit 6
    "(0,1,1)"   # Bit 7
]

def generate_lookup_table():
    table = []
    for byte in range(256):
        offsets = []
        for bit in range(8):  # Bit 0 (LSB) to Bit 7 (MSB)
            if byte & (1 << bit):
                offsets.append(f"int3{Offsets_2x2x2[bit]}")
        # Fill the remaining slots with int3(-1)
        while len(offsets) < 9:
            offsets.append("int3(-1,-1,-1)")
        # Format the entry with bits displayed from MSB to LSB
        binary_str = ''.join(['1' if byte & (1 << bit) else '0' for bit in reversed(range(8))])
        entry = f"{{ " + ", ".join(offsets) + f" }}"
        table.append(entry)
    return table

# Write to a file or print
lookup_table = generate_lookup_table()
with open("lookup_table.txt", "w") as f:
    f.write("static const uint3 _8BitToOffset_2x2x2[256][9] = {\n")
    for entry in lookup_table:
        f.write(f"    {entry},\n")
    f.write("};\n")

print("Lookup table generated and written to lookup_table.txt")

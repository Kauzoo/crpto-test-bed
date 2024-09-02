import numpy
f = numpy.fromfile(open("R:\\sink.txt"), dtype=numpy.float32)

#for n in f:
    #print(n)


#f_str_arr = numpy.array2string(f)
numstr = f.astype(str)
f2 = open("sink_readable.txt", 'w')
COUNT = 0
for nums in numstr:
    COUNT = COUNT + 1
    f2.write(f"{nums}, ")
    if (COUNT >= 256):
        f2.write("\n\n")
        print("ja moin")
        COUNT = 0 

#string_repr = ' '.join(f.astype(str))

#COUNT = 0
#f2.write(string_repr)

    




"""
Embedded Python Blocks:

Each time this file is saved, GRC will instantiate the first class it finds
to get ports and parameters of your block. The arguments to __init__  will
be the parameters. All of them are required to have default values!
"""

import numpy as np
from gnuradio import gr # type: ignore


class blk(gr.sync_block):  # other base classes are basic_block, decim_block, interp_block
    """Embedded Python Block example - a simple multiply const"""
    #PATH = "R:/sink.txt"
    #f = open(PATH, 'bw')

    def __init__(self, vectorSize=512):  # only default arguments here
        """arguments to this function show up as parameters in GRC"""
        gr.sync_block.__init__(
            self,
            name='AppendDelim',   # will show up in GRC
            in_sig=[(np.float32,vectorSize)],
            out_sig=[(np.float32,vectorSize)]
            #PATH = "B:/source/Unity/CryptoTestBed/GNURadio/sink.txt"
            #f = open(PATH, 'bw')
        )
        # if an attribute with the same name as a parameter is found,
        # a callback is registered (properties work, too).

    def work(self, input_items, output_items):
        """example: multiply with constant"""
        output_items[0][:] = input_items[0] * 1.0
        #output_items[0][len(input_items[0])-1] = output_items[0][len(input_items[0])-1] * (-1.0)
        #PATH = "B:/source/Unity/CryptoTestBed/GNURadio/sink.txt"
        #f = open(PATH, 'bw')
        #np.ndarray.tofile(f)
        PATH = "R:/sink.txt"
        f = open(PATH, 'bw')
        output_items[0].tofile(f)
        f.close()
        return len(output_items[0])
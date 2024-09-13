#!/usr/bin/env python3
# -*- coding: utf-8 -*-

#
# SPDX-License-Identifier: GPL-3.0
#
# GNU Radio Python Flow Graph
# Title: Audio-to-FFT-file
# Author: nyr
# GNU Radio version: 3.10.10.0

from gnuradio import audio
from gnuradio import blocks
from gnuradio import fft
from gnuradio.fft import window
from gnuradio import gr
from gnuradio.filter import firdes
import sys
import signal
from argparse import ArgumentParser
from gnuradio.eng_arg import eng_float, intx
from gnuradio import eng_notation
import untitled_epy_block_0 as epy_block_0  # embedded python block




class untitled(gr.top_block):

    def __init__(self):
        gr.top_block.__init__(self, "Audio-to-FFT-file", catch_exceptions=True)

        ##################################################
        # Variables
        ##################################################
        self.vec_len = vec_len = 512
        self.samp_rate = samp_rate = 48000

        ##################################################
        # Blocks
        ##################################################

        self.fft_vxx_0 = fft.fft_vfc(512, True, window.blackmanharris(512), True, 1)
        self.epy_block_0 = epy_block_0.blk(vectorSize=512)
        self.blocks_stream_to_vector_0 = blocks.stream_to_vector(gr.sizeof_float*1, vec_len)
        self.blocks_null_sink_0 = blocks.null_sink(gr.sizeof_float*vec_len)
        self.blocks_complex_to_mag_squared_0 = blocks.complex_to_mag_squared(vec_len)
        self.audio_source_0 = audio.source(samp_rate, 'NVIDIA Broadcast', True)


        ##################################################
        # Connections
        ##################################################
        self.connect((self.audio_source_0, 0), (self.blocks_stream_to_vector_0, 0))
        self.connect((self.blocks_complex_to_mag_squared_0, 0), (self.epy_block_0, 0))
        self.connect((self.blocks_stream_to_vector_0, 0), (self.fft_vxx_0, 0))
        self.connect((self.epy_block_0, 0), (self.blocks_null_sink_0, 0))
        self.connect((self.fft_vxx_0, 0), (self.blocks_complex_to_mag_squared_0, 0))


    def get_vec_len(self):
        return self.vec_len

    def set_vec_len(self, vec_len):
        self.vec_len = vec_len

    def get_samp_rate(self):
        return self.samp_rate

    def set_samp_rate(self, samp_rate):
        self.samp_rate = samp_rate




def main(top_block_cls=untitled, options=None):
    tb = top_block_cls()

    def sig_handler(sig=None, frame=None):
        tb.stop()
        tb.wait()

        sys.exit(0)

    signal.signal(signal.SIGINT, sig_handler)
    signal.signal(signal.SIGTERM, sig_handler)

    tb.start()

    try:
        input('Press Enter to quit: ')
    except EOFError:
        pass
    tb.stop()
    tb.wait()


if __name__ == '__main__':
    main()

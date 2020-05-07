# MNXtoSVG
This is a desktop application, written in C#, that converts MNX files to SVG (and embedded MIDI data).

MNX is a set of emerging music notation standards being developed by the [W3C Music Notation Community Group](https://www.w3.org/community/music-notation/).<br />
The MNCG is currently concentrating on the definition of <i><b>MNX-Common</b></i>, which is intended to supercede the latest version of [<i><b>MusicXML</b></i>](https://www.musicxml.com/).

This application has been created because I think that the only way to understand and develop such standards effectively is to actually parse the proposed XML, and use it to create <i>instantiations</i>. I agree very much with [Adrian Holovaty](https://github.com/adrianholovaty) (Co-Chair of the MNCG), that an abstract discussion of the [Draft Spec](https://w3c.github.io/mnx/specification/common/) is not going to get us much further.<br />
Insights gained from looking at real code should, however, provide invaluable feedback and promote useful debate.

<b>MNXtoSVG</b> inherits code from my [Moritz](https://github.com/notator/Moritz) repository, but Moritz was designed to use only a subset of Common Western Music Notation, so code for the missing objects is having to be added. This is being done incrementally, by instantiating the examples in [MNX-Common by Example](https://w3c.github.io/mnx/by-example/) in the order they appear. Completed examples can be found in the [SVG out folder](https://github.com/notator/MNXtoSVG/tree/master/MNX_Main/SVG_out).

The application is really designed to be used in debug mode inside Visual Studio, not as a finished tool.<br />
Its main purpose is to provide feedback about the developing standards, but it should also accumulate a set of useful music notation object definitions and, hopefully, contribute to a wider debate about music notation in general.<br />

A more detailed description of how it looks and works can be found (at this repository's github.io site). 

May 7th, 2020
 



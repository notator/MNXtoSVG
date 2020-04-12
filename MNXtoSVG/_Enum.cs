using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace MNXtoSVG.Globals
{
    public enum MNXProfileEnum
    {
        undefined,
        MNXCommonStandard
    }

    public enum MNXLineType
    {
        solid, // always default
        dashed,
        dotted
    }

    /// <summary>
    /// The first part of these names were derived from the SMuFL names at 
    /// https://w3c.github.io/smufl/gitbook/tables/individual-notes.html
    /// (SMuFL defines these 12 symbols)
    /// </summary>
    public enum MNXC_DurationSymbolType
    {
        undefined,
        noteDoubleWhole_breve,
        noteWhole_semibreve,
        noteHalf_minim,
        noteQuarter_crotchet,
        note8th_1flag_quaver,
        note16th_2flags_semiquaver,
        note32nd_3flags_demisemiquaver,
        note64th_4flags,
        note128th_5flags,
        note256th_6flags,
        note512th_7flags,
        note1024th_8flags
    }

    /// <summary>
    /// ji -- April 2020: Should three repeat barline types be defined as well?
    ///     repeat-begin,
    ///     repeat-end,
    ///     repeat-end-begin
    /// </summary>
    public enum MNXBarlineType
    {
        undefined,
        regular,
        dotted,
        dashed,
        heavy,
        lightLight,
        lightHeavy,
        heavyLight,
        heavyHeavy,
        tick,
        _short,
        none,
    }

    public enum MNXOctaveShiftType
    {
        undefined,
        down1Oct, // 8va (notes are rendered down one octave)
        up1Oct,   // 8vb (notes are rendered up one octave)
        down2Oct, // 15ma(notes are rendered down two octaves)
        up2Oct,   // 15mb(notes are rendered up two octaves)
        down3Oct, // 22ma(notes are rendered down three octaves)
        up3Oct    // 22mb(notes are rendered up three octaves)
    }

    public enum MNXOrientation
    {
        undefined,
        up,
        down
    }

    /// <summary>
    /// Avaiable CWMN accidentals copied from MusicXML. (Not all the accidentals there are for CWMN.)
    /// See https://usermanuals.musicxml.com/MusicXML/Content/ST-MusicXML-accidental-value.htm
    /// </summary>
    public enum MNXCommonAccidental
    {
        auto, // from spec
        sharp,
        natural,
        flat,
        doubleSharp,
        sharpSharp,
        flatFlat,
        naturalSharp,
        naturalFlat,
        quarterFlat,
        quarterSharp,
        threeQuartersFlat,
        threeQuartersSharp,
        sharpDown,
        sharpUp,
        naturalDown,
        naturalUp,
        flatDown,
        flatUp,
        tripleSharp,
        tripleFlat
    }

    public enum MNXClefSign
    {
        undefined, // ji
        G, // G (treble) clef
        F, // F(bass) clef
        C, // C clef
        percussion, // Percussion clef
        jianpu, // Jianpu clef ?? not mnx-common...
        tab, // not in MNX Spec... but in MusicXML spec: https://usermanuals.musicxml.com/MusicXML/Content/ST-MusicXML-clef-sign.htm
        none // The spec asks: Is the none value from MusicXML needed? Why?
    }

    public enum MNXCTupletNumberDisplay
    {
        inner, // ShowNumber default 
        both,
        none // ShowValue default
    }

    public enum MNXCTupletBracketDisplay
    {
        auto, // default 
        yes,
        no
    }

    public enum MNXCGraceType
    {
        stealPrevious,
        stealFollowing,
        makeTime
    }
}

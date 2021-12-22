namespace MNX.Common
{
    /// <summary>
    /// The first part of these names were derived from the SMuFL names at 
    /// https://w3c.github.io/smufl/gitbook/tables/individual-notes.html
    /// (SMuFL defines these 12 symbols)
    /// </summary>
    public enum DurationSymbolType
    {
        noteDoubleWhole_breve, // 8192 ticks
        noteWhole_semibreve,   // 4096 ticks
        noteHalf_minim,        // 2048 ticks
        noteQuarter_crotchet,  // 1024 ticks
        note8th_1flag_quaver,  // 512 ticks
        note16th_2flags_semiquaver, // 256 ticks
        note32nd_3flags_demisemiquaver, // 128 ticks
        note64th_4flags,  // 64 ticks
        note128th_5flags, // 32 ticks
        note256th_6flags, // 16 ticks
        note512th_7flags, // 8 ticks
        note1024th_8flags // 4 ticks
    }

    /// <summary>
    /// ji -- April 2020: Should three repeat barline types be defined as well?
    ///     repeat-begin,
    ///     repeat-end,
    ///     repeat-end-begin
    /// </summary>
    public enum BarlineType
    {
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

    public enum OctaveShiftType
    {
        down1Oct, // 8va (notes are rendered down one octave)
        up1Oct,   // 8vb (notes are rendered up one octave)
        down2Oct, // 15ma(notes are rendered down two octaves)
        up2Oct,   // 15mb(notes are rendered up two octaves)
        down3Oct, // 22ma(notes are rendered down three octaves)
        up3Oct    // 22mb(notes are rendered up three octaves)
    }

    /// <summary>
    /// Avaiable CWMN accidentals copied from MusicXML. (Not all the accidentals there are for CWMN.)
    /// See https://usermanuals.musicxml.com/MusicXML/Content/ST-MusicXML-accidental-value.htm
    /// </summary>
    public enum AccidentalType
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

    public enum ClefType
    {
        G, // G (treble) clef
        F, // F(bass) clef
        C, // C clef
        percussion, // Percussion clef
        jianpu, // Jianpu clef ?? not mnx-common...
        tab, // not in MNX Spec... but in MusicXML spec: https://usermanuals.musicxml.com/MusicXML/Content/ST-MusicXML-clef-sign.htm
        none // The spec asks: Is the none value from MusicXML needed? Why?
    }

    public enum TupletNumberDisplay
    {
        inner, // ShowNumber default 
        both,
        none // ShowValue default
    }

    public enum TupletBracketDisplay
    {
        auto, // default 
        yes,
        no
    }

    public enum GraceType
    {
        stealPrevious,
        stealFollowing,
        makeTime
    }

    public enum ShortTieOrSlur
    {
        incoming,
        outgoing
    }

    public enum LineType
    {
        solid, // always default
        dashed,
        dotted
    }

    public enum Orientation
    {
        up,
        down
    }

    public enum JumpType
    {
        unknown,
        segno,
        dsalfine
    }

    public enum BeamHookDirection
    {
        left,
        right
    }
}

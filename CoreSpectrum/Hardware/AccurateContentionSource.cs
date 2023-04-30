using CoreSpectrum.Interfaces;
using Konamiman.Z80dotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace CoreSpectrum.Hardware
{
    /// <summary>
    /// Contention pattern source based on timmings extracted from the real machine.
    /// All the information was retrieved from https://worldofspectrum.org/faq/reference/48kreference.htm
    /// </summary>
    public unsafe class AccurateContentionSource : IContentionSource
    {
        #region TABLES

        /// <summary>
        /// Contention pattern types for single-byte instructions
        /// </summary>
        static CPT[] SingleOpcode_Ops = 
		{
			       /*   0        1        2        3        4        5        6        7        8        9        A        B        C        D        E        F */
		    /* 0 */	CPT.CP01,CPT.CP13,CPT.CP08,CPT.CP04,CPT.CP01,CPT.CP01,CPT.CP07,CPT.CP01,CPT.CP01,CPT.CP05,CPT.CP08,CPT.CP04,CPT.CP01,CPT.CP01,CPT.CP07,CPT.CP01,
		    /* 1 */	CPT.CP29,CPT.CP13,CPT.CP08,CPT.CP04,CPT.CP01,CPT.CP01,CPT.CP07,CPT.CP01,CPT.CP28,CPT.CP05,CPT.CP08,CPT.CP04,CPT.CP01,CPT.CP01,CPT.CP07,CPT.CP01,
		    /* 2 */	CPT.CP28,CPT.CP13,CPT.CP17,CPT.CP04,CPT.CP01,CPT.CP01,CPT.CP07,CPT.CP01,CPT.CP28,CPT.CP05,CPT.CP17,CPT.CP04,CPT.CP01,CPT.CP01,CPT.CP07,CPT.CP01,
		    /* 3 */	CPT.CP28,CPT.CP13,CPT.CP16,CPT.CP04,CPT.CP19,CPT.CP19,CPT.CP14,CPT.CP01,CPT.CP28,CPT.CP05,CPT.CP16,CPT.CP04,CPT.CP01,CPT.CP01,CPT.CP07,CPT.CP01,
		    /* 4 */	CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP08,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP08,CPT.CP01,
		    /* 5 */	CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP08,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP08,CPT.CP01,
		    /* 6 */	CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP08,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP08,CPT.CP01,
		    /* 7 */	CPT.CP08,CPT.CP08,CPT.CP08,CPT.CP08,CPT.CP08,CPT.CP08,CPT.CP01,CPT.CP08,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP08,CPT.CP01,
		    /* 8 */	CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP09,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP09,CPT.CP01,
		    /* 9 */	CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP09,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP09,CPT.CP01,
		    /* A */	CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP09,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP09,CPT.CP01,
		    /* B */	CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP09,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP01,CPT.CP09,CPT.CP01,
		    /* C */	CPT.CP25,CPT.CP23,CPT.CP13,CPT.CP13,CPT.CP27,CPT.CP26,CPT.CP07,CPT.CP26,CPT.CP25,CPT.CP23,CPT.CP13,CPT.None,CPT.CP27,CPT.CP27,CPT.CP07,CPT.CP26,
		    /* D */	CPT.CP25,CPT.CP23,CPT.CP13,CPT.CP31,CPT.CP27,CPT.CP26,CPT.CP07,CPT.CP26,CPT.CP25,CPT.CP01,CPT.CP13,CPT.CP31,CPT.CP27,CPT.None,CPT.CP07,CPT.CP26,
		    /* E */	CPT.CP25,CPT.CP23,CPT.CP13,CPT.CP33,CPT.CP27,CPT.CP26,CPT.CP07,CPT.CP26,CPT.CP25,CPT.CP01,CPT.CP13,CPT.CP01,CPT.CP27,CPT.None,CPT.CP07,CPT.CP26,
		    /* F */	CPT.CP25,CPT.CP23,CPT.CP13,CPT.CP01,CPT.CP27,CPT.CP26,CPT.CP07,CPT.CP26,CPT.CP25,CPT.CP04,CPT.CP13,CPT.CP01,CPT.CP27,CPT.None,CPT.CP07,CPT.CP26
        };

        /// <summary>
        /// Contention pattern types for ED-prefixed instructions
        /// </summary>
        static CPT[] ED_OpcodeOps =
        {
                   /*   0        1        2        3        4        5        6        7        8        9        A        B        C        D        E        F */
            /* 0 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* 1 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* 2 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* 3 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* 4 */	CPT.CP32,CPT.CP32,CPT.CP06,CPT.CP18,CPT.CP02,CPT.None,CPT.CP02,CPT.CP03,CPT.CP32,CPT.CP32,CPT.CP06,CPT.CP18,CPT.CP02,CPT.None,CPT.CP02,CPT.CP03,
            /* 5 */	CPT.CP32,CPT.CP32,CPT.CP06,CPT.CP18,CPT.CP02,CPT.None,CPT.CP02,CPT.CP03,CPT.CP32,CPT.CP32,CPT.CP06,CPT.CP18,CPT.CP02,CPT.None,CPT.CP02,CPT.CP03,
            /* 6 */	CPT.CP32,CPT.CP32,CPT.CP06,CPT.CP18,CPT.CP02,CPT.None,CPT.CP02,CPT.CP30,CPT.CP32,CPT.CP32,CPT.CP06,CPT.CP18,CPT.CP02,CPT.None,CPT.CP02,CPT.CP30,
            /* 7 */	CPT.CP32,CPT.CP32,CPT.CP06,CPT.CP18,CPT.CP02,CPT.None,CPT.CP02,CPT.None,CPT.CP32,CPT.CP32,CPT.CP06,CPT.CP18,CPT.CP02,CPT.None,CPT.CP02,CPT.None,
            /* 8 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* 9 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* A */	CPT.CP34,CPT.CP35,CPT.None,CPT.CP37,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP34,CPT.CP35,CPT.None,CPT.CP37,CPT.None,CPT.None,CPT.None,CPT.None,
            /* B */	CPT.CP34,CPT.CP35,CPT.None,CPT.CP37,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP34,CPT.CP35,CPT.None,CPT.CP37,CPT.None,CPT.None,CPT.None,CPT.None,
            /* C */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* D */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* E */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* F */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None
        };

        /// <summary>
        /// Contention pattern types for CB-prefixed instructions
        /// </summary>
        static CPT[] CB_OpcodeOps =
        {
                   /*   0        1        2        3        4        5        6        7        8        9        A        B        C        D        E        F */
            /* 0 */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* 1 */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* 2 */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* 3 */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* 4 */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP11,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP11,CPT.CP02,
            /* 5 */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP11,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP11,CPT.CP02,
            /* 6 */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP11,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP11,CPT.CP02,
            /* 7 */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP11,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP11,CPT.CP02,
            /* 8 */ CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* 9 */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* A */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* B */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* C */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* D */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* E */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
            /* F */	CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP02,CPT.CP20,CPT.CP02,
        };

        /// <summary>
        /// Contention patterns for DD or FD prefixed instructions. If the pattern is a "None" then the pattern from the 
        /// the single byte table must be used prepending a pc:4 to the operations.
        /// </summary>
        static CPT[] DD_FD_OpcodeOps =
        {
                   /*   0        1        2        3        4        5        6        7        8        9        A        B        C        D        E        F */
            /* 0 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* 1 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* 2 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* 3 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP21,CPT.CP21,CPT.CP15,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* 4 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,
            /* 5 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,
            /* 6 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,
            /* 7 */	CPT.CP10,CPT.CP10,CPT.CP10,CPT.CP10,CPT.CP10,CPT.CP10,CPT.None,CPT.CP10,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,
            /* 8 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,
            /* 9 */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,
            /* A */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,
            /* B */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.CP10,CPT.None,
            /* C */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* D */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* E */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,
            /* F */	CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None,CPT.None
        };

        /// <summary>
        /// Contention patterns for DDCB/FDCB prefixed instructions.
        /// </summary>
        static CPT[] DDCB_FDCB_OpcodeOps =
        {
                   /*   0        1        2        3        4        5        6        7        8        9        A        B        C        D        E        F */
            /* 0 */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* 1 */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* 2 */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* 3 */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* 4 */	CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,
            /* 5 */	CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,
            /* 6 */	CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,
            /* 7 */	CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,CPT.CP12,
            /* 8 */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* 9 */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* A */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* B */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* C */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* D */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* E */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,
            /* F */	CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22,CPT.CP22
        };

        /// <summary>
        /// Contention pattern types.
        /// For our implementation some described patterns are the same, but to mantain coherence
        /// with the document all of them are implemented.
        /// </summary>
        static CP[] Contention_Patterns = 
        {
            /* pc:4 */
            new CP{ PatternType = CPT.CP01, FixedStates = 4, Operations = 
                new[]{ 
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory } 
                }  
            },
            /* pc:4,pc+1:4 */
            new CP{ PatternType = CPT.CP02, FixedStates = 8, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:5 */
            new CP{ PatternType = CPT.CP03, FixedStates = 9, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 5 }, ContentionType = CT.Memory }
                }
            },
            /* pc:6 */
            new CP{ PatternType = CPT.CP04, FixedStates = 6, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 6 }, ContentionType = CT.Memory }
                }
            },
            /* pc:11 */
            new CP{ PatternType = CPT.CP05, FixedStates = 11, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 11 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:11 */
            new CP{ PatternType = CPT.CP06, FixedStates = 15, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 11 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:3 */
            new CP{ PatternType = CPT.CP07, FixedStates = 7, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,ss:3 */
            new CP{ PatternType = CPT.CP08, FixedStates = 7, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,hl:3 */
            new CP{ PatternType = CPT.CP09, FixedStates = 7, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,pc+2:3,pc+2:1 x 5,ii+n:3 */
            new CP{ PatternType = CPT.CP10, FixedStates = 19, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1, 1, 1, 1 ,1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                }
            },
            /* pc:4,pc+1:4,hl:3,hl:1 */
            new CP{ PatternType = CPT.CP11, FixedStates = 12, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1 }, ContentionType = CT.Memory }
                }
            },
            /* pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1 */
            new CP{ PatternType = CPT.CP12, FixedStates = 20, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1, 1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:3,pc+2:3 */
            new CP{ PatternType = CPT.CP13, FixedStates = 10, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:3,hl:3 */
            new CP{ PatternType = CPT.CP14, FixedStates = 10, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3 */
            new CP{ PatternType = CPT.CP15, FixedStates = 19, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1, 1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:3,pc+2:3,nn:3 */
            new CP{ PatternType = CPT.CP16, FixedStates = 13, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:3,pc+2:3,nn:3,nn+1:3 */
            new CP{ PatternType = CPT.CP17, FixedStates = 16, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,pc+2:3,pc+3:3,nn:3,nn+1:3 */
            new CP{ PatternType = CPT.CP18, FixedStates = 20, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,hl:3,hl:1,hl(write):3 */
            new CP{ PatternType = CPT.CP19, FixedStates = 11, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,hl:3,hl:1,hl(write):3 */
            new CP{ PatternType = CPT.CP20, FixedStates = 15, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,pc+2:3,pc+2:1 x 5,ii+n:3,ii+n:1,ii+n(write):3 */
            new CP{ PatternType = CPT.CP21, FixedStates = 23, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1, 1, 1, 1, 1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,pc+2:3,pc+3:3,pc+3:1 x 2,ii+n:3,ii+n:1,ii+n(write):3 */
            new CP{ PatternType = CPT.CP22, FixedStates = 23, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1, 1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,sp:3,sp+1:3 */
            new CP{ PatternType = CPT.CP23, FixedStates = 10, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,sp:3,sp+1:3 */
            new CP{ PatternType = CPT.CP24, FixedStates = 14, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:5,[sp:3,sp+1:3] */
            new CP{ PatternType = CPT.CP25, FixedStates = 5, OptionalStates = 11, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 5 }, ContentionType = CT.Memory },
                    new CO{ OptionalRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ OptionalRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                }
            },
            /* pc:5,sp-1:3,sp-2:3 */
            new CP{ PatternType = CPT.CP26, FixedStates = 11, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 5 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:3,pc+2:3,[pc+2:1,sp-1:3,sp-2:3] */
            new CP{ PatternType = CPT.CP27, FixedStates = 10, OptionalStates = 17, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, OptionalRuns = new byte[]{ 1 }, ContentionType = CT.Memory },
                    new CO{ OptionalRuns = new byte[]{ 3, 3 }, ContentionType = CT.Memory },
                }
            },
            /* pc:4,pc+1:3,[pc+1:1 x 5] */
            new CP{ PatternType = CPT.CP28, FixedStates = 7, OptionalStates = 12, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, OptionalRuns = new byte[]{ 1,1,1,1,1 }, ContentionType = CT.Memory }
                }
            },
            /* pc:5,pc+1:3,[pc+1:1 x 5] */
            new CP{ PatternType = CPT.CP29, FixedStates = 8, OptionalStates = 13, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 5 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, OptionalRuns = new byte[]{ 1,1,1,1,1 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,hl:3,hl:1 x 4,hl(write):3 */
            new CP{ PatternType = CPT.CP30, FixedStates = 18, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1, 1, 1, 1 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:3,IO */
            new CP{ PatternType = CPT.CP31, FixedStates = 11, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.IO }
                }
            },
            /* pc:4,pc+1:4,IO */
            new CP{ PatternType = CPT.CP32, FixedStates = 12, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.IO }
                }
            },
            /* pc:4,sp:3,sp+1:4,sp(write):3,sp+1(write):3,sp+1(write):1 x 2 */
            new CP{ PatternType = CPT.CP33, FixedStates = 19, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1, 1 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,hl:3,de:3,de:1 x 2,[de:1 x 5] */
            new CP{ PatternType = CPT.CP34, FixedStates = 16, OptionalStates = 21, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1, 1 }, OptionalRuns = new byte[]{ 1, 1, 1, 1, 1 },  ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:4,hl:3,hl:1 x 5,[hl:1 x 5] */
            new CP{ PatternType = CPT.CP35, FixedStates = 16, OptionalStates = 21, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3, 1, 1, 1, 1, 1 }, OptionalRuns = new byte[]{ 1, 1, 1, 1, 1 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:5,IO,hl:3,[hl:1 x 5] */
            new CP{ PatternType = CPT.CP36, FixedStates = 16, OptionalStates = 21, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 5 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.IO },
                    new CO{ FixedRuns = new byte[]{ 3 }, OptionalRuns = new byte[]{ 1, 1, 1, 1, 1 }, ContentionType = CT.Memory }
                }
            },
            /* pc:4,pc+1:5,hl:3,IO,[hl:1 x 5] */
            new CP{ PatternType = CPT.CP37, FixedStates = 16, OptionalStates = 21, Operations =
                new[]{
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 5 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 3 }, ContentionType = CT.Memory },
                    new CO{ FixedRuns = new byte[]{ 4 }, ContentionType = CT.IO },
                    new CO{ OptionalRuns = new byte[]{ 1, 1, 1, 1, 1 }, ContentionType = CT.Memory }
                }
            },
        };

        /// <summary>
        /// Spectrum model types
        /// </summary>
        static SpectrumModelTimmings[] Spectrum_Models =
        {
            //48k
            new SpectrumModelTimmings(false, 224, 312, 64, 255, 14335),
            //128k/+2
            new SpectrumModelTimmings(true, 228, 311, 63, 254, 14361)
        };

        #endregion
        #region Table structs and enums
        /// <summary>
        /// Contention pattern type
        /// </summary>
        private enum CPT
        {
            /*
            Pattern types as documetned in https://worldofspectrum.org/faq/reference/48kreference.htm#Contention

            Instruction    Breakdown
            -----------    ---------
            */
            CP01,
            /*
                NOP            pc:4
                LD r,r' 
                alo A,r 
                INC/DEC r 
                EXX 
                EX AF,AF' 
                EX DE,HL 
                DAA 
                CPL 
                CCF 
                SCF 
                DI 
                EI 
                RLA 
                RRA 
                RLCA 
                RRCA 
                JP (HL)
                HALT
            */
            CP02,
            /*
                NOPD           pc:4,pc1:4
                sro r
                BIT b,r 
                SET b,r 
                RES b,r 
                NEG 
                IM 0/1/2 
            */
            CP03,
            /*
                LD A,I         pc:4,pc1:5
                LD A,R 
                LD I,A 
                LD R,A
            */
            CP04,
            /*
                INC/DEC dd     pc:6
                LD SP,HL
            */
            CP05,
            /*
                ADD HL,dd      pc:11
            */
            CP06,
            /*
                ADC HL,dd      pc:4,pc1:11
                SBC HL,dd
            */
            CP07,
            /*
                LD r,n         pc:4,pc1:3
                alo A,n
            */
            CP08,
            /*
                LD r,(ss)      pc:4,ss:3
                LD (ss),r
            */
            CP09,
            /*
                alo A,(HL)     pc:4,hl:3
                alo (HL)
            */
            CP10,
            /*
                LD r,(iin)    pc:4,pc1:4,pc2:3,pc2:1 x 5,iin:3
                LD (iin),r    
                alo A,(iin)
                alo (iin)
            */
            CP11,
            /*
                BIT b,(HL)     pc:4,pc1:4,hl:3,hl:1
            */
            CP12,
            /*
                BIT b,(iin)   pc1:4,pc2:3,pc3:3,pc3:1 x 2,iin:3,iin:1
                WARNING!!!!
                This pattern is incorrectly documented, it should be;
                pc:4,pc1:4,pc2:3,pc3:3,pc3:1 x 2,iin:3,iin:1
            */
            CP13,
            /*
                LD dd,nn       pc:4,pc1:3,pc2:3
                JP nn 
                JP cc,nn
            */
            CP14,
            /*
                LD (HL),n      pc:4,pc1:3,hl:3
            */
            CP15,
            /*
                LD (iin),n    pc:4,pc1:4,pc2:3,pc3:3,pc3:1 x 2,iin:3
            */
            CP16,
            /*
                LD A,(nn)      pc:4,pc1:3,pc2:3,nn:3
                LD (nn),A

            */
            CP17,
            /*
                The following entry applies to the unprefixed version of these
                opcodes (22 and 2A)
                LD HL,(nn)     pc:4,pc1:3,pc2:3,nn:3,nn1:3
                LD (nn),HL
            */
            CP18,
            /*
                The following entry applies to the prefixed version of these
                opcodes (ED43, ED4B, ED53, ED5B, ED63, ED6B, ED73 and ED7B)
                LD dd,(nn)     pc:4,pc1:4,pc2:3,pc3:3,nn:3,nn1:3
                LD (nn),dd
            */
            CP19,
            /*
                INC/DEC (HL)   pc:4,hl:3,hl:1,hl(write):3
            */
            CP20,
            /*
                SET b,(HL)     pc:4,pc1:4,hl:3,hl:1,hl(write):3
                RES b,(HL) 
                sro (HL)
            */
            CP21,
            /*
                INC/DEC (iin) pc:4,pc1:4,pc2:3,pc2:1 x 5,iin:3,iin:1,iin(write):3
            */
            CP22,
            /*
                SET b,(iin)   pc:4,pc1:4,pc2:3,pc3:3,pc3:1 x 2,iin:3,iin:1,iin(write):3
                RES b,(iin)   
                sro (iin)
            */
            CP23,
            /*
                POP dd         pc:4,sp:3,sp1:3
                RET 
            */
            CP24,
            /*
                RETI           pc:4,pc1:4,sp:3,sp1:3
                RETN
            */
            CP25,
            /*
                RET cc         pc:5,[sp:3,sp1:3]
            */
            CP26,
            /*
                PUSH dd        pc:5,sp-1:3,sp-2:3
                RST n
            */
            CP27,
            /*
                CALL nn        pc:4,pc1:3,pc2:3,[pc2:1,sp-1:3,sp-2:3]
                CALL cc,nn     
            */
            CP28,
            /*
                JR n           pc:4,pc1:3,[pc1:1 x 5]
                JR cc,n        
            */
            CP29,
            /*
                DJNZ n         pc:5,pc1:3,[pc1:1 x 5]
            */
            CP30,
            /*
                RLD            pc:4,pc1:4,hl:3,hl:1 x 4,hl(write):3
                RRD
            */
            CP31,
            /*
                IN A,(n)       pc:4,pc1:3,IO
                OUT (n),A
            */
            CP32,
            /*
                IN r,(C)       pc:4,pc1:4,IO
                OUT (C),r
            */
            CP33,
            /*
                EX (SP),HL     pc:4,sp:3,sp1:4,sp(write):3,sp1(write):3,sp1(write):1 x 2
            */
            CP34,
            /*
                LDI/LDIR       pc:4,pc1:4,hl:3,de:3,de:1 x 2,[de:1 x 5]
                LDD/LDDR       
            */
            CP35,
            /*
                CPI/CPIR       pc:4,pc1:4,hl:3,hl:1 x 5,[hl:1 x 5]
                CPD/CPDR       
            */
            CP36,
            /*
                INI/INIR       pc:4,pc1:5,IO,hl:3,[hl:1 x 5]
                IND/INDR
            */
            CP37,
            /*
                OUTI/OTIR      pc:4,pc1:5,hl:3,IO,[hl:1 x 5]
                OUTD/OTDR
            */
            None
        }

        /// <summary>
        /// Contention pattern
        /// </summary>
        private struct CP
        {
            public CPT PatternType;
            /// <summary>
            /// Fixed number of states that the operation must take. If the operation takes
            /// more states then all the optional operation runs are applied
            /// </summary>
            public byte FixedStates;
            /// <summary>
            /// Number of optional states (total sum), used(?) for sanity check
            /// </summary>
            public byte OptionalStates;
            public CO[] Operations;
        }

        /// <summary>
        /// Contended operation
        /// A contended operation is an access to the memory/io.
        /// Each operation can be a source of contention multiple times in operations like (hl:3, hl:1) or (hl:1 x 5)
        /// </summary>
        private struct CO
        {
            public byte[]? FixedRuns;
            public byte[]? OptionalRuns;
            public CT ContentionType;
        }

        /// <summary>
        /// Contention type
        /// </summary>
        public enum CT
        {
            Memory,
            IO
        }

        /// <summary>
        /// Spectrum model timmings
        /// </summary>
        struct SpectrumModelTimmings
        {
            public bool Is128k;
            public ulong ScanTStates;
            public ulong ScansPerFrame;
            public ulong FirstScan;
            public ulong LastScan;
            public ulong FrameTStates;
            public ulong FirstContendedTState;

            public SpectrumModelTimmings(bool Is128k, ulong ScanTStates, ulong ScansPerFrame, ulong FirstScan, ulong LastScan, ulong FirstContendedTState)
            {
                this.Is128k = Is128k;
                this.ScanTStates = ScanTStates;
                this.ScansPerFrame = ScansPerFrame;
                this.FirstScan = FirstScan;
                this.LastScan = LastScan;
                this.FrameTStates = ScansPerFrame * ScanTStates;
                this.FirstContendedTState = FirstContendedTState;
            }
        }

        #endregion

        /// <summary>
        /// Contention states per scan line
        /// </summary>
        byte[] Scan_Contention_States;

        /// <summary>
        /// Our speccy model :)
        /// </summary>
        SpectrumModelTimmings currentModel;

        /// <summary>
        /// Constructor, do you really need help to understand what it is? ¬¬
        /// </summary>
        /// <param name="Is128k">C'mon man...</param>
        public AccurateContentionSource(bool Is128k)
        {
            if (Is128k)
                currentModel = Spectrum_Models[1];
            else
                currentModel = Spectrum_Models[0];

            //Initialize array with contention states for a scanline
            //Only the first 128 TStates may have contention, we create the full array for speeding up calculations
            Scan_Contention_States = new byte[currentModel.ScanTStates];

            //Pattern for 48k/128k/+2 models goes 6,5,4,3,2,1,0,0 and repeats until tState 128, then it has 0 contention states for the
            //rest of the scanline
            for (int cycle = 0; cycle < 16; cycle++)
            {
                for (int state = 0; state < 8; state++)
                {
                    int index = state + cycle * 8;

                    switch (state)
                    {
                        case 0:
                            Scan_Contention_States[index] = 6;
                            break;
                        case 1:
                            Scan_Contention_States[index] = 5;
                            break;
                        case 2:
                            Scan_Contention_States[index] = 4;
                            break;
                        case 3:
                            Scan_Contention_States[index] = 3;
                            break;
                        case 4:
                            Scan_Contention_States[index] = 2;
                            break;
                        case 5:
                            Scan_Contention_States[index] = 1;
                            break;
                        case 6:
                        case 7:
                            Scan_Contention_States[index] = 0;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Get the contention states for an opcode execution (Who would have figured it!)
        /// </summary>
        /// <param name="InitialState">Initial state at which the instruction execution started</param>
        /// <param name="ExecutionStates">Number of TStates used by its execution</param>
        /// <param name="OpCode">Byte array with the op-code</param>
        /// <param name="MemoryAccesses">List of memory accesses</param>
        /// <param name="PortAccesses">List off port accesses</param>
        /// <param name="Memory">The memory device used to execute this instruction</param>
        /// <returns>Maybe the contention states? Not sure... :P</returns>
        /// <exception cref="ArgumentException">Bad argument... ^_^</exception>
        public int GetContentionStates(ulong InitialState, int ExecutionStates, byte[] OpCode, ushort[] MemoryAccesses, (byte PortHi, byte PortLo)[] PortAccesses, IMemory Memory)
        {
            CPT patternType = CPT.None;
            bool extraPrefix = false;

            if (OpCode.Length < 1)
                throw new ArgumentException("Invalid opcode!");
            else
            {
                if (OpCode.Length > 1)
                {
                    if (OpCode[0] == 0xCB)
                        patternType = CB_OpcodeOps[OpCode[1]];
                    else if (OpCode[0] == 0xED)
                        patternType = ED_OpcodeOps[OpCode[1]];
                    else if (OpCode[0] == 0xDD || OpCode[0] == 0xFD)
                    {
                        if (OpCode[1] == 0xCB)
                            patternType = DDCB_FDCB_OpcodeOps[OpCode[3]];
                        else
                        {
                            patternType = DD_FD_OpcodeOps[OpCode[1]];
                            if (patternType == CPT.None)
                            {
                                patternType = SingleOpcode_Ops[OpCode[1]];
                                extraPrefix = true;
                            }
                        }
                    }
                    else
                    {
                        if (OpCode[0] == 0 && MemoryAccesses.Length == 0)
                            return 0;

                        patternType = SingleOpcode_Ops[OpCode[0]];
                    }
                }
                else
                {
                    if (OpCode[0] == 0 && MemoryAccesses.Length == 0)
                        return 0;

                    patternType = SingleOpcode_Ops[OpCode[0]];
                }
            }

            if (patternType == CPT.None)
                return 0; //Maybe return 4? :P

            return ProcessPatternType(patternType, InitialState, ExecutionStates, MemoryAccesses, PortAccesses, Memory, extraPrefix);
        }

        /// <summary>
        /// Process the patter that applies to the opcode
        /// </summary>
        /// <param name="patternType">The pattern type</param>
        /// <param name="initialState">Initial state at which the instruction execution started</param>
        /// <param name="executionStates">Number of TStates used by its execution</param>
        /// <param name="memoryAccesses">List of memory accesses</param>
        /// <param name="portAccesses">List off port accesses</param>
        /// <param name="memory">The memory device used to execute this instruction</param>
        /// <param name="extraPrefix">If true it indicates that an extra fetch op must be prepended to the pattern ops</param>
        /// <returns>Number of cycles eaten by the contention</returns>
        /// <exception cref="InvalidOperationException">Will happen if the execution states of the op don't match with the ones specified in the pattern</exception>
        private unsafe int ProcessPatternType(CPT patternType, ulong initialState, int executionStates, ushort[] memoryAccesses, (byte PortHi, byte PortLo)[] portAccesses, IMemory memory, bool extraPrefix)
        {

            //TODO: Implement a fast discard based on the initial state

            int index = (int)patternType;

            int fStates = Contention_Patterns[index].FixedStates + (extraPrefix ? 4 : 0);
            int oStates = Contention_Patterns[index].OptionalStates + (extraPrefix ? 4 : 0);

            bool includeOptional;

            if (executionStates == fStates)
                includeOptional = false;
            else if (executionStates == oStates)
                includeOptional = true;
            else
                return 0;//throw new InvalidOperationException("OpCode execution length mismatched!");

            ulong tState = initialState;
            var ops = Contention_Patterns[index].Operations;

            int portOpIndex = 0;
            int memOpIndex = 0;

            if (extraPrefix)
            {
                tState += GetMemoryContention(memoryAccesses[memOpIndex++], tState, memory);
                tState += 4;
            }

            for(int opNumber = 0; opNumber < ops.Length; opNumber++) 
            {
                var op = ops[opNumber];

                if (op.FixedRuns != null)
                {
                    byte[] runs = op.FixedRuns;

                    for (int runNumber = 0; runNumber < runs.Length; runNumber++)
                    {
                        if (op.ContentionType == CT.IO)
                            tState += GetPortContention(portAccesses[portOpIndex], tState);
                        else
                            tState += GetMemoryContention(memoryAccesses[memOpIndex], tState, memory);

                        tState += runs[runNumber];
                    }
                }

                if (includeOptional && op.OptionalRuns != null)
                {
                    byte[] runs = op.OptionalRuns;

                    for (int runNumber = 0; runNumber < runs.Length; runNumber++)
                    {
                        if (op.ContentionType == CT.IO)
                            tState += GetPortContention(portAccesses[portOpIndex], tState);
                        else
                            tState += GetMemoryContention(memoryAccesses[memOpIndex], tState, memory);

                        tState += runs[runNumber];
                    }
                }

                if (op.ContentionType == CT.IO)
                    portOpIndex++;
                else
                    memOpIndex++;
            }

            return (int)(tState - (initialState + (ulong)executionStates));
        }

        /// <summary>
        /// Get contention states for a memory access
        /// </summary>
        /// <param name="address">Memory access address</param>
        /// <param name="tState">TStates when the access happened</param>
        /// <param name="memory">The memory device used for this access</param>
        /// <returns>Number of TStates wasted in contention</returns>
        private ulong GetMemoryContention(ushort address, ulong tState, IMemory memory)
        {
            if (currentModel.Is128k)
            {
                //Check if mem is 128k? Are we so stupid? How it will impact performance?
                var mem = (Memory128k)memory;
                
                if ((address > 16383 && address < 32768) || (address >= 0xC000 && (mem.Map.ActiveBank & 1) == 1)) //Contention may happen
                    return GetContention(tState);

                return 0;
            }
            else
            {
                if (address > 16383 && address < 32768) //Contention may happen
                    return GetContention(tState);

                return 0;
            }
        }

        /// <summary>
        /// Get contention TStates for a port access
        /// </summary>
        /// <param name="port">Port address</param>
        /// <param name="tState">TStates when the access happened</param>
        /// <returns>Number of TStates wasted in contention</returns>
        private ulong GetPortContention((byte PortHi, byte PortLo) port, ulong tState)
        {
            //Information about IO contention:

            //It takes four T states for the Z80 to read a value from an I/O port, or write a value to a port. As is the case with memory access, this can be lengthened by the ULA. There are two effects which occur here:

            //    If the port address being accessed has its low bit reset, the ULA is required to supply the result, which leads to a delay if it is currently busy handling the screen.
            //    The address of the port being accessed is placed on the data bus. If this is in the range 0x4000 to 0x7fff, the ULA treats this as an attempted access to contended memory and therefore introduces a delay. If the port being accessed is between 0xc000 and 0xffff, this effect does not apply, even on a 128K machine if a contended memory bank is paged into the range 0xc000 to 0xffff.


            //These two effects combine to lead to the following contention patterns:

            //    High byte   |         | 
            //    in 40 - 7F? | Low bit | Contention pattern  
            //    ------------+---------+-------------------
            //         No     |  Reset  | N:1, C:3
            //         No     |   Set   | N:4
            //        Yes     |  Reset  | C:1, C:3
            //        Yes     |   Set   | C:1, C:1, C:1, C:1

            //The 'Contention pattern' column should be interpreted from left to right. An "N:n" entry means that no delay is applied at this cycle, and the Z80 continues uninterrupted for 'n' T states. A "C:n" entry means that the ULA halts the Z80; the delay is exactly the same as would occur for a contended memory access at this cycle (eg 6 T states at cycle 14335, 5 at 14336, etc on the 48K machine). After this delay, the Z80 then continues for 'n' cycles.

            bool ulaHandled = (port.PortLo & 1) == 0;
            bool ulaContended = port.PortHi >= 0x40 && port.PortHi <= 0x7F;

            //Second pattern, no contention at all
            if (!ulaHandled && !ulaContended)
                return 0;

            if (ulaHandled && !ulaContended)
            {
                //First pattern, return contention for T+1
                return GetContention(tState + 1);
            }
            else if (ulaHandled && ulaContended)
            {
                //Third pattern, returns contention for (T) + (T+1+contention)
                ulong accumulatedStates = GetContention(tState);
                accumulatedStates += GetContention(tState + 1 + (ulong)accumulatedStates);
                return accumulatedStates;

            }
            else // if (!ulaHandled && ulaContended)
            {
                //Fourth pattern, returns contention for (T) + (T+1+contention) + (T+2+contention) + (T+3+contention)
                ulong accumulatedStates = GetContention(tState);
                accumulatedStates += GetContention(tState + 1 + (ulong)accumulatedStates);
                accumulatedStates += GetContention(tState + 2 + (ulong)accumulatedStates);
                accumulatedStates += GetContention(tState + 3 + (ulong)accumulatedStates);
                return accumulatedStates;
            }
        }

        /// <summary>
        /// Gets the contention TStates at a certain TState
        /// </summary>
        /// <param name="tState">TState to get its contention cycles</param>
        /// <returns>C'mon Sherlock, you can guess it!</returns>
        byte GetContention(ulong tState)
        {
            
            //Get current TState in a frame cycle
            //Why I added +3? ¯\_(0_o)_/¯, does it works? CHECK IT!
            //var frameTStates = (tState + 3) % currentModel.FrameTStates;
            //Ok, it is because the INT happens 3 cycles BEFORE the scan starts (¯ー¯)b
            var frameTStates = (tState + 3) % currentModel.FrameTStates;

            //Get current scanline
            var scanLine = frameTStates / currentModel.ScanTStates;

            //Fixed (¯ー¯)b
            frameTStates -= 3;

            //If the scanline is not in the border...
            //Check first and last scan, does the documentation refer to it based on zero or one?
            if (scanLine >= currentModel.FirstScan && scanLine <= currentModel.LastScan)
            {
                //Get the TState in the scan cycle
                //Initial FrameTState had +3? ¯\_(o_0)_/¯
                //var scanTState = (int)((frameTStates - 14364) % ScanTStates);
                //Fixed (¯ー¯)b
                var scanTState = (int)((frameTStates - currentModel.FirstContendedTState) % currentModel.ScanTStates);
                //Return the contention states
                return Scan_Contention_States[scanTState];
            }

            return 0;
        }

        /*
         * Code removed from the ULA
         * 
        //128k
        const ulong ScanTStates = 228;
        const ulong ScansPerFrame = 311;
        const ulong FirstScan = 63;
        const ulong LastScan = 254;
        const ulong FrameTStates = ScansPerFrame * ScanTStates;


        //48k
        const ulong ScanTStates = 224;
        const ulong ScansPerFrame = 312;
        const ulong FirstScan = 64;
        const ulong LastScan = 255;
        const ulong FrameTStates = ScansPerFrame * ScanTStates;

        private int[] ScanContentionStates;

        
            
            //Initialize array with contention states for a scanline
            //Only the first 128 TStates may have contention, we create the full array for speeding up calculations
            ScanContentionStates = new int[ScanTStates];

            for (int cycle = 0; cycle < 16; cycle++)
            {
                for (int state = 0; state < 8; state++)
                {
                    int index = state + cycle * 8;

                    switch (state)
                    {
                        case 0:
                            ScanContentionStates[index] = 6;
                            break;
                        case 1:
                            ScanContentionStates[index] = 5;
                            break;
                        case 2:
                            ScanContentionStates[index] = 4;
                            break;
                        case 3:
                            ScanContentionStates[index] = 3;
                            break;
                        case 4:
                            ScanContentionStates[index] = 2;
                            break;
                        case 5:
                            ScanContentionStates[index] = 1;
                            break;
                        case 6:
                        case 7:
                            ScanContentionStates[index] = 0;
                            break;
                    }
                }
            }


        
        internal override int GetPortWaitStates(byte PortHi, byte PortLo, ulong TStates, bool IsWrite)
        {
            if (ESCUPE)
            {
                System.Diagnostics.Debug.WriteLine($"Check waits en puerto {PortHi.ToString("X2")}-{PortLo.ToString("X2")}");

                bool ulaHandleds = (PortLo & 1) == 0;
                bool ulaContendeds = PortHi >= 0x40 && PortHi <= 0x7F;
                int accumulatedStates = 0;
                //Second pattern, no contention at all
                if (!ulaHandleds && !ulaContendeds)
                    return 0;

                if (ulaHandleds && !ulaContendeds)
                {
                    //First pattern, return contention for T+1
                    accumulatedStates = GetContentionStates(TStates + 1);
                }
                else if (ulaHandleds && ulaContendeds)
                {
                    //Third pattern, returns contention for (T) + (T+1+contention)
                    accumulatedStates = GetContentionStates(TStates);
                    accumulatedStates += GetContentionStates(TStates + 1 + (ulong)accumulatedStates);

                }
                else if (!ulaHandleds && ulaContendeds)
                {
                    //Fourth pattern, returns contention for (T) + (T+1+contention) + (T+2+contention) + (T+3+contention)
                    accumulatedStates = GetContentionStates(TStates);
                    accumulatedStates += GetContentionStates(TStates + 1 + (ulong)accumulatedStates);
                    accumulatedStates += GetContentionStates(TStates + 2 + (ulong)accumulatedStates);
                    accumulatedStates += GetContentionStates(TStates + 3 + (ulong)accumulatedStates);
                }

                System.Diagnostics.Debug.WriteLine($"Waits en puerto:{accumulatedStates}");

                return accumulatedStates;
            }

            //Information about IO contention:

            //It takes four T states for the Z80 to read a value from an I/O port, or write a value to a port. As is the case with memory access, this can be lengthened by the ULA. There are two effects which occur here:

            //    If the port address being accessed has its low bit reset, the ULA is required to supply the result, which leads to a delay if it is currently busy handling the screen.
            //    The address of the port being accessed is placed on the data bus. If this is in the range 0x4000 to 0x7fff, the ULA treats this as an attempted access to contended memory and therefore introduces a delay. If the port being accessed is between 0xc000 and 0xffff, this effect does not apply, even on a 128K machine if a contended memory bank is paged into the range 0xc000 to 0xffff.


            //These two effects combine to lead to the following contention patterns:

            //    High byte   |         | 
            //    in 40 - 7F? | Low bit | Contention pattern  
            //    ------------+---------+-------------------
            //         No     |  Reset  | N:1, C:3
            //         No     |   Set   | N:4
            //        Yes     |  Reset  | C:1, C:3
            //        Yes     |   Set   | C:1, C:1, C:1, C:1

            //The 'Contention pattern' column should be interpreted from left to right. An "N:n" entry means that no delay is applied at this cycle, and the Z80 continues uninterrupted for 'n' T states. A "C:n" entry means that the ULA halts the Z80; the delay is exactly the same as would occur for a contended memory access at this cycle (eg 6 T states at cycle 14335, 5 at 14336, etc on the 48K machine). After this delay, the Z80 then continues for 'n' cycles.


            bool ulaHandled = (PortLo & 1) == 0;
            bool ulaContended = PortHi >= 0x40 && PortHi <= 0x7F;

            //Second pattern, no contention at all
            if (!ulaHandled && !ulaContended)
                return 0;

            if (ulaHandled && !ulaContended)
            {
                //First pattern, return contention for T+1
                return GetContentionStates(TStates + 1);
            }
            else if (ulaHandled && ulaContended)
            {
                //Third pattern, returns contention for (T) + (T+1+contention)
                int accumulatedStates = GetContentionStates(TStates);
                accumulatedStates += GetContentionStates(TStates + 1 + (ulong)accumulatedStates);
                return accumulatedStates;

            }
            else if (!ulaHandled && ulaContended)
            {
                //Fourth pattern, returns contention for (T) + (T+1+contention) + (T+2+contention) + (T+3+contention)
                int accumulatedStates = GetContentionStates(TStates);
                accumulatedStates += GetContentionStates(TStates + 1 + (ulong)accumulatedStates);
                accumulatedStates += GetContentionStates(TStates + 2 + (ulong)accumulatedStates);
                accumulatedStates += GetContentionStates(TStates + 3 + (ulong)accumulatedStates);
                return accumulatedStates;
            }
            return 0;
        }

        internal override int GetMemoryWaitStates(ushort Address, ulong TStates, bool IsWrite)
        {
            if (ESCUPE)
            {
                System.Diagnostics.Debug.WriteLine($"Check waits en memoria {Address.ToString("X4")}");

                int contentionStates = 0;

                if ((Address > 16383 && Address < 32768) || (Address >= 0xC000 && (_mem.Map.ActiveBank & 1) == 1)) //Contention may happen
                    contentionStates = GetContentionStates(TStates);

                System.Diagnostics.Debug.WriteLine($"Waits en memoria: {contentionStates}");

                return contentionStates;
            }

            if ((Address > 16383 && Address < 32768) || (Address >= 0xC000 && (_mem.Map.ActiveBank & 1) == 1)) //Contention may happen
                return 6;// GetContentionStates(TStates);

            return 0;
        }

        private int GetContentionStates(ulong TStates)
        {

            //Get current TState in a frame cycle
            var frameTStates = (TStates + 3) % FrameTStates;
            //Get current scanline
            var scanLine = frameTStates / ScanTStates;

            //If the scanline is not in the border...
            if (scanLine >= FirstScan && scanLine <= LastScan)
            {
                //Get the TState in the scan cycle
                var scanTState = (int)((frameTStates - 14364) % ScanTStates);
                //Return the contention states
                return ScanContentionStates[scanTState];
            }

            return 0;
        }

         */
    }


}

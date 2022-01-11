using MNX.Globals;
using Moritz.Spec;
using System;
using System.Collections.Generic;
using System.Xml;

namespace MNX.Common
{
    public class BeamBlock : IUniqueDef
    {
        #region IUniqueDef interface
        public override string ToString() {return "BeamBlock";}

        /// <summary>
        /// Returns a deep clone (a unique object)
        /// </summary>
        public object Clone()
        {
            BeamBlock clone = new BeamBlock();
            foreach(var component in Components)
            {
                if(component is Beam beam)
                {
                    var beamClone = new Beam(beam.EventIDs, component.Depth);
                    clone.Components.Add(beamClone);
                }
                else if(component is BeamHook beamHook)
                {
                    var beamHookClone = new BeamHook(beamHook.EventID, beamHook.BeamHookDirection, beamHook.Depth);
                    clone.Components.Add(beamHookClone);

                }
            }

            return clone;
        }

        /// <summary>
        /// Multiplies the MsDuration by the given factor.
        /// </summary>
        /// <param name="factor"></param>
        public void AdjustMsDuration(double factor){ }
        public int MsDuration { get { return 0; } set { } }
        public int MsPositionReFirstUD { get; set; }
        #endregion
        public readonly List<IBeamBlockComponent> Components = new List<IBeamBlockComponent>();

        public BeamBlock()
        {
        }
    }
}

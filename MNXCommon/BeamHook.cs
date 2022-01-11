using System.Xml;
using MNX.Globals;
using Moritz.Spec;

namespace MNX.Common
{
    // https://w3c.github.io/mnx/specification/common/#the-time-element
    public class BeamHook : IBeamBlockComponent
    {
        public readonly string EventID;
        public readonly BeamHookDirection BeamHookDirection;
        public int Depth { get; }

        public override string ToString() => $"Depth={Depth} EventID={EventID} BeamHookDirection={BeamHookDirection}";

        /// <summary>
        /// clone constructor
        /// </summary>
        /// <param name="eventID"></param>
        /// <param name="beamHookDirection"></param>
        /// <param name="depth"></param>
        public BeamHook(string eventID, BeamHookDirection beamHookDirection, int depth)
        {
            EventID = eventID;
            BeamHookDirection = beamHookDirection;
            Depth = depth;
        }

        public BeamHook(XmlReader r, int topLevelDepth)
        {
            M.Assert(r.Name == "beam-hook");

            Depth = r.Depth - topLevelDepth;

            int count = r.AttributeCount;
            for(int i = 0; i < count; i++)
            {
                r.MoveToAttribute(i);
                switch(r.Name)
                {
                    case "event":
                        {
                            EventID = r.Value.Trim();
                            break;
                        }
                    case "direction":
                        {
                            switch(r.Value.Trim())
                            {
                                case "left":
                                    BeamHookDirection = BeamHookDirection.left;
                                    break;
                                case "right":
                                    BeamHookDirection = BeamHookDirection.right;
                                    break;
                            }
                            break;
                        }
                    default:
                        M.ThrowError("Unknown beam-hook attribute.");
                        break;
                }
            }

            r.MoveToElement();
            M.Assert(r.Name == "beam-hook");
        }
    }
}
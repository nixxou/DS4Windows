﻿/*
DS4Windows
Copyright (C) 2023  Travis Nickles

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace DS4Windows
{
    public class Xbox360OutDevice : OutputDevice
    {
        [Flags]
        public enum X360Features : ushort
        {
            XInputSlotNum = 0x01
        }

        //private const int inputResolution = 127 - (-128);
        //private const float reciprocalInputResolution = 1 / (float)inputResolution;
        private const float recipInputPosResolution = 1 / 127f;
        private const float recipInputNegResolution = 1 / 128f;
        private const int outputResolution = 32767 - (-32768);
        public const string devType = "X360";
        private const int USER_INDEX_WAIT = 250;
        public IXbox360Controller cont;
        //public Xbox360FeedbackReceivedEventHandler forceFeedbackCall;
        // Input index, Xbox360FeedbackReceivedEventHandler instance
        public Dictionary<int, Xbox360FeedbackReceivedEventHandler> forceFeedbacksDict =
            new Dictionary<int, Xbox360FeedbackReceivedEventHandler>();

        private const int XINPUT_SLOT_NUM_DEFAULT = -1;
        private const int MAX_XINPUT_SLOT_NUM = 8;
        private int _xInputSlotNum = XINPUT_SLOT_NUM_DEFAULT;
        public int XinputSlotNum
        {
            get => _xInputSlotNum;
            set
            {
                if (value >= 0 && value < MAX_XINPUT_SLOT_NUM) _xInputSlotNum = value;
            }
        }

        private X360Features _features;
        public X360Features Features => _features;

        public Xbox360OutDevice(ViGEmClient client)
        {
            cont = client.CreateXbox360Controller();
            cont.AutoSubmitReport = false;
        }

		public Xbox360OutDevice(ViGEmClient client, ushort vendorId, ushort productId)
		{
			cont = client.CreateXbox360Controller(vendorId,productId);
			cont.AutoSubmitReport = false;
		}

		public Xbox360OutDevice(ViGEmClient client, X360Features features) :
            this(client)
        {
            this._features = features;
        }

		public Xbox360OutDevice(ViGEmClient client, X360Features features, ushort vendorId, ushort productId) :
	        this(client,vendorId,productId)
		{
			this._features = features;
		}

		public override void ConvertandSendReport(DS4State state, int device)
        {
            if (!connected) return;

            //cont.ResetReport();
            ushort tempButtons = 0;

            unchecked
            {
                if (state.Share) tempButtons |= Xbox360Button.Back.Value;
                if (state.L3) tempButtons |= Xbox360Button.LeftThumb.Value;
                if (state.R3) tempButtons |= Xbox360Button.RightThumb.Value;
                if (state.Options) tempButtons |= Xbox360Button.Start.Value;

                if (state.DpadUp) tempButtons |= Xbox360Button.Up.Value;
                if (state.DpadRight) tempButtons |= Xbox360Button.Right.Value;
                if (state.DpadDown) tempButtons |= Xbox360Button.Down.Value;
                if (state.DpadLeft) tempButtons |= Xbox360Button.Left.Value;

                if (state.L1) tempButtons |= Xbox360Button.LeftShoulder.Value;
                if (state.R1) tempButtons |= Xbox360Button.RightShoulder.Value;

                if (state.Triangle) tempButtons |= Xbox360Button.Y.Value;
                if (state.Circle) tempButtons |= Xbox360Button.B.Value;
                if (state.Cross) tempButtons |= Xbox360Button.A.Value;
                if (state.Square) tempButtons |= Xbox360Button.X.Value;
                if (state.PS) tempButtons |= Xbox360Button.Guide.Value;
                cont.SetButtonsFull(tempButtons);
            }

            cont.LeftTrigger = state.L2;
            cont.RightTrigger = state.R2;

            SASteeringWheelEmulationAxisType steeringWheelMappedAxis = Global.GetSASteeringWheelEmulationAxis(device);
            switch (steeringWheelMappedAxis)
            {
                case SASteeringWheelEmulationAxisType.None:
                    cont.LeftThumbX = AxisScale(state.LX, false);
                    cont.LeftThumbY = AxisScale(state.LY, true);
                    cont.RightThumbX = AxisScale(state.RX, false);
                    cont.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.LX:
                    cont.LeftThumbX = (short)state.SASteeringWheelEmulationUnit;
                    cont.LeftThumbY = AxisScale(state.LY, true);
                    cont.RightThumbX = AxisScale(state.RX, false);
                    cont.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.LY:
                    cont.LeftThumbX = AxisScale(state.LX, false);
                    cont.LeftThumbY = (short)state.SASteeringWheelEmulationUnit;
                    cont.RightThumbX = AxisScale(state.RX, false);
                    cont.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.RX:
                    cont.LeftThumbX = AxisScale(state.LX, false);
                    cont.LeftThumbY = AxisScale(state.LY, true);
                    cont.RightThumbX = (short)state.SASteeringWheelEmulationUnit;
                    cont.RightThumbY = AxisScale(state.RY, true);
                    break;

                case SASteeringWheelEmulationAxisType.RY:
                    cont.LeftThumbX = AxisScale(state.LX, false);
                    cont.LeftThumbY = AxisScale(state.LY, true);
                    cont.RightThumbX = AxisScale(state.RX, false);
                    cont.RightThumbY = (short)state.SASteeringWheelEmulationUnit;
                    break;

                case SASteeringWheelEmulationAxisType.L2R2:
                    cont.LeftTrigger = cont.RightTrigger = 0;
                    if (state.SASteeringWheelEmulationUnit >= 0) cont.LeftTrigger = (Byte)state.SASteeringWheelEmulationUnit;
                    else cont.RightTrigger = (Byte)state.SASteeringWheelEmulationUnit;
                    goto case SASteeringWheelEmulationAxisType.None;

                case SASteeringWheelEmulationAxisType.VJoy1X:
                case SASteeringWheelEmulationAxisType.VJoy2X:
                    DS4Windows.VJoyFeeder.vJoyFeeder.FeedAxisValue(state.SASteeringWheelEmulationUnit, ((((uint)steeringWheelMappedAxis) - ((uint)SASteeringWheelEmulationAxisType.VJoy1X)) / 3) + 1, DS4Windows.VJoyFeeder.HID_USAGES.HID_USAGE_X);
                    goto case SASteeringWheelEmulationAxisType.None;

                case SASteeringWheelEmulationAxisType.VJoy1Y:
                case SASteeringWheelEmulationAxisType.VJoy2Y:
                    DS4Windows.VJoyFeeder.vJoyFeeder.FeedAxisValue(state.SASteeringWheelEmulationUnit, ((((uint)steeringWheelMappedAxis) - ((uint)SASteeringWheelEmulationAxisType.VJoy1X)) / 3) + 1, DS4Windows.VJoyFeeder.HID_USAGES.HID_USAGE_Y);
                    goto case SASteeringWheelEmulationAxisType.None;

                case SASteeringWheelEmulationAxisType.VJoy1Z:
                case SASteeringWheelEmulationAxisType.VJoy2Z:
                    DS4Windows.VJoyFeeder.vJoyFeeder.FeedAxisValue(state.SASteeringWheelEmulationUnit, ((((uint)steeringWheelMappedAxis) - ((uint)SASteeringWheelEmulationAxisType.VJoy1X)) / 3) + 1, DS4Windows.VJoyFeeder.HID_USAGES.HID_USAGE_Z);
                    goto case SASteeringWheelEmulationAxisType.None;

                default:
                    // Should never come here but just in case use the NONE case as default handler....
                    goto case SASteeringWheelEmulationAxisType.None;
            }

            cont.SubmitReport();
        }

        private short AxisScale(Int32 Value, Boolean Flip)
        {
            unchecked
            {
                Value -= 0x80;
                float recipRun = Value >= 0 ? recipInputPosResolution : recipInputNegResolution;

                float temp = Value * recipRun;
                //if (Flip) temp = (temp - 0.5f) * -1.0f + 0.5f;
                if (Flip) temp = -temp;
                temp = (temp + 1.0f) * 0.5f;

                return (short)(temp * outputResolution + (-32768));
            }
        }

        public override void Connect()
        {
            cont.Connect();
            connected = true;

            if (_features.HasFlag(X360Features.XInputSlotNum))
            {
                // Need a delay here
                Thread.Sleep(USER_INDEX_WAIT);
                try
                {
                    XinputSlotNum = cont.UserIndex;
                }
                catch (Exception)
                {
                    // Failed to grab xinput slot number. Set default
                    // slot number and remove feature flag
                    _xInputSlotNum = XINPUT_SLOT_NUM_DEFAULT;
                    _features -= X360Features.XInputSlotNum;
                }
            }
        }
        public override void Disconnect()
        {
            foreach (KeyValuePair<int, Xbox360FeedbackReceivedEventHandler> pair in forceFeedbacksDict)
            {
                cont.FeedbackReceived -= pair.Value;
            }

            forceFeedbacksDict.Clear();

            connected = false;
            cont.Disconnect();
            cont = null;
        }
        public override string GetDeviceType() => devType;

        public override void ResetState(bool submit=true)
        {
            cont.ResetReport();
            if (submit)
            {
                cont.SubmitReport();
            }
        }

        public override void RemoveFeedbacks()
        {
            foreach (KeyValuePair<int, Xbox360FeedbackReceivedEventHandler> pair in forceFeedbacksDict)
            {
                cont.FeedbackReceived -= pair.Value;
            }

            forceFeedbacksDict.Clear();
        }

        public override void RemoveFeedback(int inIdx)
        {
            if (forceFeedbacksDict.TryGetValue(inIdx, out Xbox360FeedbackReceivedEventHandler handler))
            {
                cont.FeedbackReceived -= handler;
                forceFeedbacksDict.Remove(inIdx);
            }
        }
    }
}

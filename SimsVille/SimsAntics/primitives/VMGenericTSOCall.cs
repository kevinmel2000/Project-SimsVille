﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0.
 * If a copy of the MPL was not distributed with this file, You can obtain one at
 * http://mozilla.org/MPL/2.0/. 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSO.SimAntics.Engine;
using FSO.Files.Utils;
using FSO.SimAntics.Model;
using System.IO;
using FSO.SimAntics.Model.TSOPlatform;
using FSO.SimAntics.NetPlay.Drivers;

namespace FSO.SimAntics.Primitives
{

    public class VMGenericTSOCall : VMPrimitiveHandler
    {
        public override VMPrimitiveExitCode Execute(VMStackFrame context, VMPrimitiveOperand args)
        {
            var operand = (VMGenericTSOCallOperand)args;

            switch (operand.Call)
            {
                // 0. HOUSE TUTORIAL COMPLETE
                case VMGenericTSOCallMode.SwapMyAndStackObjectsSlots: //1
                    var cont1 = context.Caller.Container;
                    var cont2 = context.StackObject.Container;
                    var contS1 = context.Caller.ContainerSlot;
                    var contS2 = context.StackObject.ContainerSlot;
                    if (cont1 != null && cont2 != null)
                    {
                        context.Caller.PrePositionChange(context.VM.Context);
                        context.StackObject.PrePositionChange(context.VM.Context);
                        cont1.ClearSlot(contS1);
                        cont2.ClearSlot(contS2);
                        cont1.PlaceInSlot(context.StackObject, contS1, true, context.VM.Context);
                        cont2.PlaceInSlot(context.Caller, contS2, true, context.VM.Context);
                    }
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.SetActionIconToStackObject: //2
                    context.Thread.Queue[0].IconOwner = context.StackObject;
                    return VMPrimitiveExitCode.GOTO_TRUE;
                // 3. DO NOT USE
                case VMGenericTSOCallMode.IsStackObjectARoommate: //4 
                    return (context.StackObject is VMGameObject 
                        || ((VMTSOAvatarState)context.StackObject.TSOState).Permissions < VMTSOAvatarPermissions.Roommate)
                        ? VMPrimitiveExitCode.GOTO_FALSE : VMPrimitiveExitCode.GOTO_TRUE;
                // 5. Combine Assets of family in Temp0
                // 6. Remove From Family
                // 7. Make New Neighbour
                // 8. Family Sims1 Tutorial Complete
                // 9. Architecture Sims1 Tutorial Complete
                // 10. Disable Build/Buy
                // 11. Enable Build/Buy
                // 12. Distance to camera in Temp0
                // 13. Abort Interactions
                // 14. House Radio Station equals Temp0 (TODO)
                case VMGenericTSOCallMode.HouseRadioStationEqualsTemp0:
                    context.VM.SetGlobalValue(31, context.Thread.TempRegisters[0]);
                    return VMPrimitiveExitCode.GOTO_TRUE;
                // 15. My Routing Footprint equals Temp0
                // 16. Change Normal Output
                case VMGenericTSOCallMode.GetInteractionResult: //17
                    context.Thread.TempRegisters[0] = 2; //0=none, 1=reject, 2=accept, 3=pet
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.SetInteractionResult: //18
                    //todo: set interaction result to value of temp 0. UNUSED.
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.DoIOwnThisObject: //19
                    context.Thread.TempRegisters[0] = (context.StackObject is VMAvatar
                    || ((VMTSOObjectState)context.StackObject.TSOState).OwnerID != context.Caller.PersistID)
                    ? (short)0 : (short)1;
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.DoesTheLocalMachineSimOwnMe: //20
                    context.Thread.TempRegisters[1] = 1; //TODO
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.MakeMeStackObjectsOwner: //21
                    if (context.StackObject is VMAvatar) return VMPrimitiveExitCode.GOTO_TRUE;
                    ((VMTSOObjectState)context.StackObject.TSOState).OwnerID = context.Caller.PersistID;
                    return VMPrimitiveExitCode.GOTO_TRUE;
                //TODO: may need to update in global server
                // 22. Get Permissions (TODO)
                // 23. Set Permissions (TODO)
                case VMGenericTSOCallMode.AskStackObjectToBeRoommate: //24
                    //0 = initiate. 1 = accept. 2 = reject.
                    if (context.Thread.TempRegisters[0] == 1 && context.VM.GlobalLink != null) context.VM.GlobalLink.RequestRoommate(context.VM, (VMAvatar)context.StackObject);
                    return VMPrimitiveExitCode.GOTO_TRUE;

                case VMGenericTSOCallMode.LeaveLot: //25
                    ((VMAvatar)context.Caller).UserLeaveLot();
                    if (context.VM.GlobalLink != null)
                    {
                        context.VM.GlobalLink.LeaveLot(context.VM, (VMAvatar)context.Caller);
                    }
                    else
                    {
                        // use our stub to remove the sim and potentially disconnect the client.
                        context.VM.CheckGlobalLink.LeaveLot(context.VM, (VMAvatar)context.Caller);
                    }
                    return VMPrimitiveExitCode.GOTO_TRUE;

                // 26. UNUSED
                case VMGenericTSOCallMode.KickoutRoommate:
                    if (context.VM.GlobalLink != null) context.VM.GlobalLink.RemoveRoommate(context.VM, (VMAvatar)context.StackObject);
                    return VMPrimitiveExitCode.GOTO_TRUE;

                case VMGenericTSOCallMode.KickoutVisitor:
                    if (context.VM.GlobalLink != null)
                    {
                        var server = (VMServerDriver)context.VM.Driver;
                        server.KickUser(context.VM, context.StackObject.Name);
                    }
                    ((VMAvatar)context.StackObject).UserLeaveLot();
                    return VMPrimitiveExitCode.GOTO_TRUE;

                case VMGenericTSOCallMode.StackObjectOwnerID: //29
                    //attempt to find owner on lot. null stack object if not present
                    if (context.StackObject is VMAvatar) context.Thread.TempRegisters[0] = 0;
                    else
                    {
                        var owner = context.VM.GetObjectByPersist(((VMTSOObjectState)context.StackObject.TSOState).OwnerID);
                        context.Thread.TempRegisters[0] = (owner == null) ? (short)0 : owner.ObjectID;
                    }
                    return VMPrimitiveExitCode.GOTO_TRUE;

                //30. Create Cheat Neighbour
                case VMGenericTSOCallMode.IsTemp0AvatarIgnoringTemp1Avatar: //31
                    context.Thread.TempRegisters[0] = 0;
                    return VMPrimitiveExitCode.GOTO_TRUE;
                //32. Play Next Song on Radio Station in Temp 0 (TODO)
                //33. Temp0 Avatar Unignore Temp1 Avatar
                case VMGenericTSOCallMode.GlobalRepairCostInTempXL0: //34
                    context.Thread.TempXL[0] = 0; //TODO
                    return VMPrimitiveExitCode.GOTO_TRUE;
                //35. Global Repair State (TODO)
                case VMGenericTSOCallMode.IsGlobalBroken: //36
                    return VMPrimitiveExitCode.GOTO_FALSE; //TODO
                // 37. UNUSED
                case VMGenericTSOCallMode.MayAddRoommate: //38
                    // TODO: Make only lot owner able to do this
                    // for testing purposes, build roommates are kind of "global moderators"
                    // also, can't add roommate if we have 8 roommates.
                    context.Thread.TempRegisters[0] = 2;
                    // 2 is "true". not sure what 1 is. (interaction shows up, but fails on trying to run it. likely "guessed" state for client)
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.ReturnLotCategory: //39
                    context.Thread.TempRegisters[0] = 6; //skills lot. see #Lot Types in global.iff
                    //TODO: set based on lot state
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.TestStackObject: //40
                    return (context.StackObject != null) ? VMPrimitiveExitCode.GOTO_TRUE : VMPrimitiveExitCode.GOTO_FALSE;
                case VMGenericTSOCallMode.GetCurrentValue: //41
                    //stack object price in TempXL 0
                    context.Thread.TempXL[0] = context.StackObject.MultitileGroup.Price;
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.IsRegionEmpty: //42
                    //is temp0 radius (in full tiles) around stack object empty?
                    //used for resurrect. TODO.
                    return VMPrimitiveExitCode.GOTO_TRUE;
                //43. Set Spotlight Status (TODO: GLOBAL SERVER)
                //44. Is Full Refund (TODO: small grace period after purchase/until user exits buy mode)
                //45. Refresh buy/build (TODO? we probably don't need this)
                case VMGenericTSOCallMode.GetLotOwner: //46
                    // TODO! global lot state not in yet
                    context.Thread.TempRegisters[0] = context.Caller.ObjectID;
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.CopyDynObjNameFromTemp0ToTemp1: //47
                    var obj1 = context.VM.GetObjectById(context.Thread.TempRegisters[0]);
                    var obj2 = context.VM.GetObjectById(context.Thread.TempRegisters[1]);
                    obj2.Name = obj1.Name;
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.GetIsPendingDeletion: //48
                    return VMPrimitiveExitCode.GOTO_FALSE;
                //49. Pet Ran Away
                //50. Set Original Purchase Price (TODO: parrot, snowman. should set to temp0)
                case VMGenericTSOCallMode.HasTemporaryID: //51
                    //persist ID should be 0 til we get one.
                    return (context.StackObject.PersistID == 0)?VMPrimitiveExitCode.GOTO_TRUE:VMPrimitiveExitCode.GOTO_FALSE;
                case VMGenericTSOCallMode.SetStackObjOwnerToTemp0: //52
                    var obj = context.VM.GetObjectById(context.Thread.TempRegisters[0]); 
                    if (context.StackObject is VMAvatar || obj == null) return VMPrimitiveExitCode.GOTO_TRUE;

                    ((VMTSOObjectState)context.StackObject.TSOState).OwnerID = obj.PersistID;
                    return VMPrimitiveExitCode.GOTO_TRUE;
                //53. Is On Editable Tile
                //54. Set Stack Object's Crafter Name To Avatar in Temp 0
                case VMGenericTSOCallMode.CalcHarvestComponents: //55
                    //TODO: where does this come from?
                    context.Thread.TempRegisters[0] = 1;
                    context.Thread.TempRegisters[1] = 1;
                    context.Thread.TempRegisters[2] = 1;
                    return VMPrimitiveExitCode.GOTO_TRUE;
                case VMGenericTSOCallMode.IsStackObjectForSale: //56. TODO
                    return VMPrimitiveExitCode.GOTO_FALSE;

                //TODO: may need to update in global server
                default:
                    return VMPrimitiveExitCode.GOTO_TRUE;
            }
        }
    }

    public class VMGenericTSOCallOperand : VMPrimitiveOperand
    {
        public VMGenericTSOCallMode Call;

        #region VMPrimitiveOperand Members
        public void Read(byte[] bytes)
        {
            using (var io = IoBuffer.FromBytes(bytes, ByteOrder.LITTLE_ENDIAN))
            {
                Call = (VMGenericTSOCallMode)io.ReadByte();
            }
        }

        public void Write(byte[] bytes) {
            using (var io = new BinaryWriter(new MemoryStream(bytes)))
            {
                io.Write((byte)Call);
            }
        }
        #endregion
    }
}

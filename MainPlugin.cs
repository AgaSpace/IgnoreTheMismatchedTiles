using System;
using Terraria;
using TerrariaApi.Server;
using System.IO;
using TShockAPI;

namespace AntiCheats
{
	[ApiVersion(2, 1)]
	public class IgnoreTheMismatchedTiles : TerrariaPlugin
	{
		public override string Author => "Zoom L1 | Colag";
		public override string Name => "Ignore the mismatched tiles";
		
		public IgnoreTheMismatchedTiles(Main game) : base(game) {}
		
		public override void Initialize()
		{
			GetDataHandlers.TileEdit += OnTileEdit;
			GetDataHandlers.PlaceObject += OnPlaceObject;
			ServerApi.Hooks.NetGetData.Register(this, OnGetData);
		}
		
		public static void OnTileEdit(object sender, GetDataHandlers.TileEditEventArgs args) 
		{
			if (args.Action == GetDataHandlers.EditAction.PlaceTile || args.Action == GetDataHandlers.EditAction.ReplaceTile)	
			{
				// The History plugin does not record 127 tiles.
				// Cheaters use this to deliver tiles without leaving traces.
				if (Main.tile[args.X, args.Y] != null && Main.tile[args.X, args.Y].type == 127)
				{
					args.Handled = true;
					return;
				}
				if (args.Player.SelectedItem.placeStyle != args.Style)
				{
					TShock.Log.ConsoleError(string.Format("/ OnTileEdit / Player {0} ({1}) set a tile whose style({2}) is not true.", args.Player.Name, args.Player.IP, args.Style));
					
					args.Player.SendErrorMessage("You cannot place this tile."); //I'm worried about real players. My plugin notifies players.
					
					args.Handled = true;
					return;
				}
			}
		}

		public static void OnPlaceObject(object sender, GetDataHandlers.PlaceObjectEventArgs args)
		{
			if (Main.tile[args.X, args.Y] != null && Main.tile[args.X, args.Y].type == 127)
			{
				args.Handled = true;
				return;
			}
			if (args.Player.SelectedItem.placeStyle != args.Style)
			{
				TShock.Log.ConsoleError(string.Format("/ OnPlaceObject / Player {0} ({1}) placed an object whose style({2}) does not match the present.", args.Player.Name, args.Player.IP, args.Style));
				
				args.Player.SendErrorMessage("You cannot place this object.");
				
				args.Handled = true;
				return;
			}
		}
		
		public static void OnGetData(GetDataEventArgs e) // tShock do not use the block delivery style in GetDateNandlers HandlePlaceChest. Therefore, I have to get the style myself.
		{
			if (e.MsgID == (PacketTypes)34) // PacketTypes: PlaceChest = 34
			{
				TSPlayer player = TShock.Players[e.Msg.whoAmI];
				using (var data = new BinaryReader(new MemoryStream(e.Msg.readBuffer, e.Index, e.Length)))
				{
					data.ReadByte(); // Action. We do not take it, since it will not be needed in any way. Breaking the chest, we send 17 packet, in our case 34 packet only puts the chest.
					int x = (int)data.ReadInt16();
					int y = (int)data.ReadInt16();
					short style = data.ReadInt16();
					if (Main.tile[x, y] != null && Main.tile[x, y].type == 127)
					{
						e.Handled = true;
						return;
					}
					if (player.SelectedItem.placeStyle != style)
					{
						TShock.Log.ConsoleError(string.Format("/ OnPlaceChest / Player {0} ({1}) placed an chest whose style({2}) does not match the present.", player.Name, player.IP, style));
						
						player.SendErrorMessage("You cannot place this chest.");
						
						e.Handled = true;
						return;
					}
				}
			}
		}
	}
}

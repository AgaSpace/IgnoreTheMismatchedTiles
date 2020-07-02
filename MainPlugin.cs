using System;
using Terraria;
using TerrariaApi.Server;
using System.IO;
using TShockAPI;

namespace KekDevelops.Plugins.AntiCheat
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
			if (args.Action == GetDataHandlers.EditAction.PlaceTile)		
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
					SendInfo(string.Format("/ OnTileEdit / Player {0} ({1}) set a tile whose style is not true.", args.Player.Name, args.Player.IP));
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
				SendInfo(string.Format("/ OnPlaceObject / Player {0} ({1}) placed an object whose style does not match the present.", args.Player.Name, args.Player.IP));
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
					data.ReadByte(); // Action.
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
						SendInfo(string.Format("/ OnPlaceChest / Player {0} ({1}) placed an chest whose style does not match the present.", player.Name, player.IP));
						e.Handled = true;
						return;
					}
				}
			}
		}
		
		private static void SendInfo(string loliline)
		{
			Console.WriteLine(loliline);
			TShock.Log.Warn(loliline);
		}
	}
}
﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace vdb_node_wireguard_manipulator
{
	public static class CommandsExecutor
	{
		private static async Task<string> RunCommand(string command, string fileName= @"wg")
		{
			var psi = new ProcessStartInfo();
			psi.FileName = fileName;

			psi.Arguments = command;
			psi.RedirectStandardOutput = true;
			psi.UseShellExecute = false;
			psi.CreateNoWindow = true;

			var process = Process.Start(psi);

			if (process is null)
			{
				throw new AggregateException("Unable to perform the command");
			}

			await process.WaitForExitAsync();

			var output = await process.StandardOutput.ReadToEndAsync();

			return output;
		}


		private static string GetAddPeerCommand(string pubKey, string allowedIps)
		{
			return $"set wg0 peer \"{pubKey}\" allowed-ips {allowedIps}";
		}
		private static string GetRemovePeerCommand(string pubKey)
		{
			return $"set wg0 peer \"{pubKey}\" remove";
		}
		private static string GetWgShowCommand(string wgInterfaceName=null!)
		{
			return wgInterfaceName is null ?
				"show" : $"wg show {wgInterfaceName}";
		}

		public static async Task<string> AddPeer(string pubKey, string allowedIps)
		{
			return await RunCommand(GetAddPeerCommand(pubKey, allowedIps));
		}
		public static async Task<string> RemovePeer(string pubKey)
		{
			return await RunCommand(GetRemovePeerCommand(pubKey));
		}
		public static async Task<string> GetPeersListUnparsed()
		{
			return await RunCommand(GetWgShowCommand());
		}
		public static  async Task<(List<WgFullPeerInfo> peers, List<WgInterfaceInfo> interfaces)> GetPeersList()
		{
			var result =  WgStatusParser.ParsePeersFromWgShow(
				await RunCommand(GetWgShowCommand()), out var ifs);
			return (result, ifs);
		}

		public static async Task<List<WgShortPeerInfo>> GetPeersListShortly()
		{
			return WgStatusParser.ParsePeersFromWgShow(
				await RunCommand(GetWgShowCommand()),out _)
				.Select(WgShortPeerInfo.FromFullInfo).ToList();
		}
	}
}

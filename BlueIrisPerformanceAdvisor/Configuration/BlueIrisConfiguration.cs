﻿using BPUtil;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Text;

namespace BlueIrisPerformanceAdvisor.Configuration
{
	public class BlueIrisConfiguration
	{
		public byte[] BiVersionBytes = new byte[4];
		public string BiVersionFromRegistry;
		public string OS;
		public string AdvisorVersion;
		public BIGlobalConfig global;
		public SortedList<string, Camera> cameras = new SortedList<string, Camera>();
		public List<GpuInfo> gpus;
		public CpuInfo cpu;
		public RamInfo mem;
		public BIActiveStats activeStats;
		public BlueIrisConfiguration()
		{
		}

		public void Load()
		{
			int version = RegistryUtil.GetHKLMValue<int>(@"SOFTWARE\Perspective Software\Blue Iris", "version", 0);
			ByteUtil.WriteInt32(version, BiVersionBytes, 0);
			BiVersionFromRegistry = string.Join(".", BiVersionBytes);
			OS = GetOsVersion();
			AdvisorVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

			RegistryKey camerasKey = RegistryUtil.GetHKLMKey(@"SOFTWARE\Perspective Software\Blue Iris\Cameras");
			if (camerasKey != null)
			{
				global = new BIGlobalConfig();
				global.Load();

				foreach (string camName in camerasKey.GetSubKeyNames())
					cameras.Add(camName, new Camera(camName, camerasKey.OpenSubKey(camName)));
				cpu = CpuInfo.GetCpuInfo();
				gpus = GpuInfo.GetGpuInfo();
				mem = RamInfo.GetRamInfo();

				activeStats = new BIActiveStats();
				activeStats.Load();
			}
		}

		public static string GetOsVersion()
		{
			StringBuilder sb = new StringBuilder();
			string prodName = RegistryUtil.GetHKLMValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "Unknown");
			sb.Append(prodName);

			string release = RegistryUtil.GetHKLMValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ReleaseId", "");
			if (string.IsNullOrWhiteSpace(release))
				sb.Append(" v" + RegistryUtil.GetHKLMValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", "Unknown"));
			else
			{
				sb.Append(" v" + release);
				string build = RegistryUtil.GetHKLMValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", "");
				if (string.IsNullOrWhiteSpace(build))
					build = RegistryUtil.GetHKLMValue(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", "");
				if (!string.IsNullOrWhiteSpace(build))
					sb.Append(" b" + build);
			}
			if (Environment.Is64BitOperatingSystem)
				sb.Append(" (64 bit)");
			else
				sb.Append(" (32 bit)");
			return sb.ToString();
		}
	}
}
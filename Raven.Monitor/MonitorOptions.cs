﻿// -----------------------------------------------------------------------
//  <copyright file="MonitorOptions.cs" company="Hibernating Rhinos LTD">
//      Copyright (c) Hibernating Rhinos LTD. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------
namespace Raven.Monitor
{
	internal class MonitorOptions
	{
		public MonitorOptions()
		{
			IoOptions = new IoOptions();
		}

		public MonitorActions Action { get; set; }

		public int ProcessId { get; set; }

		public string OutputPath { get; set; }

		public IoOptions IoOptions { get; private set; }
	}

	internal class IoOptions
	{
		public int DurationInMinutes { get; set; }
	}
}
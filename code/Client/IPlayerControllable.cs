﻿// SPDX-FileCopyrightText: 2022 Admer Šuko
// SPDX-License-Identifier: MIT

namespace Bodot.Client
{
	// What the server sends back to the client
	public struct PlayerControllerState
	{
		public Vector3 Position;
		public Vector3 Angles; // mmm not actually needed but oh well
	}

	public interface IPlayerControllable
	{
		void HandleClientInput( ClientCommands commands );
		PlayerControllerState GenerateControllerState();
	}
}

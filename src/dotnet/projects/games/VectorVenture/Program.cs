// Copyright (c) Lucas Girouard-Stranks (https://github.com/lithiumtoast). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the Git repository root directory (https://github.com/lithiumtoast/sokol-cs/) for full license information.

namespace VectorVenture
{
	public class Program
	{
		public static void Main(string[] args)
		{
			using var game = new VectorVentureGame();
			game.Run();
		}
	}
}

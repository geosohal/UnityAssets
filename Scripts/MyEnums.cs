namespace MyEnums
{
	public enum StarCollectorEventTypes : byte
	{
		ReceivePlayerID = 0,
		CreateActor = 1,
		DestroyActor = 2,
		UpdateActor = 3,
		ChatMessage = 4
	}

	public enum StarCollectorRequestTypes : byte
	{
		MoveCommand = 0
	}
}
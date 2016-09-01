package script_fumble
{
	function Observer::onTrigger(%this, %obj, %triggerNum, %val,%a,%b)
	{
		parent::onTrigger(%this, %obj, %triggerNum, %val,%a,%b);
		if(isObject(%client = %obj.getControllingClient()) && isEventPending(%client.fumbleSchedule) && (%client.canFumble || %client.isAdmin))
		switch(%triggerNum)
		{
			case 4:
			if(!%val)
			{
				cancel(%client.fumbleSchedule);
				%client.player.unMount();
				%client.setControlObject(%client.player);
			}
			case 0:
			%client.FumbleRotation = %val;
		}
	}
};
activatePackage(script_fumble);
function servercmdCanFumble(%client,%find)
{
	if(!%client.isSuperAdmin)
		return;
	if(%find $= "")
	{
		%client.chatMessage("\c6/canFumble [Name/BLID OR Toggle]");
		return;
	}
	if(%find $= "toggle")
	{
		$canFumble = !$canFumble;
		announce("\c6Fumble is now "@ ($canFumble ? "enabled":"disabled"));
		return;
	}
	if(!isObject(%victim = findclientbyBL_ID(%find)))
		if(!isObject(%victim = findclientbyname(%find)))
		{
			%client.chatMessage("\c6Could not find "@ %find @" by BLID or Name");
			return;
		}
	%victim.canFumble = !%victim.canFumble;
	%client.chatMessage("\c6"@ %victim.getPlayerName() @" can "@ (%victim.canFumble ? "now fumble" : "no longer fumble"));
	%victim.chatMessage("\c6You can "@ (%victim.canFumble ? "now fumble" : "no longer fumble"));
}
function servercmdfumble(%client,%str)
{
	if((!%client.isAdmin && !%client.canFumble) || !isObject(%player = %client.player))
		return;
	if(!$canFumble)
	{
		%client.chatMessage("\c6Fumble is disabled.");
		return;
	}
	if(getSimTime() - %client.canFumbleLast < 2000)
	{
		%client.chatMessage("\c6Please wait before using this command.");
		return;
	}
	%client.canFumbleLast = getSimTime();
	cancel(%client.tumbleSchedule);
	%tum = %player.getObjectMount();
	if(!isObject(%tum))
	{
		tumble(%player,1);
		%tum = %player.getObjectMount();
	}
	%client.fumbleSchedule(%str);
}
function GameConnection::fumbleSchedule(%client,%str)
{
	cancel(%client.fumbleSchedule);
	if(!isObject(%client) || !isObject(%player = %client.player) || (!%client.isAdmin && !%client.canFumble))
		return;
	if(!isObject(%tum = %player.getObjectMount()) || !isObject(%ctrl = %client.getControlObject()))
		return;
	%vel = vectorAdd(%tum.getVelocity(),vectorScale(%ctrl.getEyeVector(),%str));
	%vel = vectorScale(vectorNormalize(%vel),%str);
	%tum.setVelocity(%vel);
    if(%client.FumbleRotation)
        %tum.setAngularVelocity(getRandom(-%str,%str) SPC getRandom(-%str,%str) SPC getRandom(-%str,%str));
	%client.fumbleSchedule = %client.schedule(50,fumbleSchedule,%str);
}

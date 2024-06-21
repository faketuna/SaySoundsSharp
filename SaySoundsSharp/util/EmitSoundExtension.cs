using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory.DynamicFunctions;

namespace SaySoundsSharp;


// Code from Metapyziks. Edit by faketuna.
// https://gist.github.com/Metapyziks/88745748d90f8955e60b9831a7214cd2
public static class EmitSoundExtension
{
    // Search string with "DeathCry" and the function using this argument is EmitSoundParams.
    private static MemoryFunctionVoid<CBaseEntity, string, int, float, float> CBaseEntity_EmitSoundParamsFunc = new("48 8B C4 48 89 58 10 48 89 70 18 55 57 41 56 48 8D A8 08");
    private static MemoryFunctionWithReturn<nint, nint, uint, uint, short, ulong, ulong> CSoundOpGameSystem_StartSoundEventFunc = new("40 57 41 54 41 55 41 56 41 57 48 81");
    private static MemoryFunctionVoid<nint, nint, ulong, nint, nint, short, byte> CSoundOpGameSystem_SetSoundEventParamFunc = new("48 89 5C 24 08 48 89 6C 24 10 56 57 41 56 48 83 EC 40 41");

    internal static void Init()
    {
        CSoundOpGameSystem_StartSoundEventFunc.Hook( CSoundOpGameSystem_StartSoundEventFunc_PostHook, HookMode.Post );
    }

    internal static void CleanUp()
    {
        CSoundOpGameSystem_StartSoundEventFunc.Unhook( CSoundOpGameSystem_StartSoundEventFunc_PostHook, HookMode.Post );
    }

    [ThreadStatic]
    private static IReadOnlyDictionary<string, float>? CurrentParameters;

    /// <summary>
    /// Emit a sound event by name (e.g., "Weapon_AK47.Single").
    /// TODO: parameters passed in here only seem to work for sound events shipped with the game, not workshop ones.
    /// </summary>
    public static void EmitSound( this CBaseEntity entity, string soundName, IReadOnlyDictionary<string, float>? parameters = null )
    {
        if ( !entity.IsValid )
        {
            throw new ArgumentException( "Entity is not valid." );
        }

        try
        {
            // We call CBaseEntity::EmitSoundParams,
            // which calls a method that returns an ID that we can use
            // to modify the playing sound.

            CurrentParameters = parameters;

            // Pitch, volume etc aren't actually used here
            CBaseEntity_EmitSoundParamsFunc.Invoke( entity, soundName, 100, 1f, 0f );
        }
        finally
        {
            CurrentParameters = null;
        }
    }

    private static HookResult CSoundOpGameSystem_StartSoundEventFunc_PostHook( DynamicHook hook )
    {
        if ( CurrentParameters is not { Count: > 0 } )
        {
            return HookResult.Continue;
        }

        var pSoundOpGameSystem = hook.GetParam<nint>( 0 );
        var pFilter = hook.GetParam<nint>( 1 );
        var soundEventId = hook.GetReturn<ulong>();

        foreach ( var parameter in CurrentParameters )
        {
            CSoundOpGameSystem_SetSoundEventParam( pSoundOpGameSystem, pFilter,
                soundEventId, parameter.Key, parameter.Value );
        }

        return HookResult.Continue;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct FloatParamData
    {
        // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
        private readonly uint _type1;
        private readonly uint _type2;

        private readonly uint _size1;
        private readonly uint _size2;

        private readonly float _value;
        private readonly uint _padding;
        // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

        public FloatParamData( float value )
        {
            _type1 = 1;
            _type2 = 8;

            _size1 = 4;
            _size2 = 4;

            _value = value;
            _padding = 0;
        }
    }

    private static unsafe void CSoundOpGameSystem_SetSoundEventParam( nint pSoundOpGameSystem, nint pFilter,
        ulong soundEventId, string paramName, float value )
    {
        var data = new FloatParamData( value );
        var nameByteCount = Encoding.UTF8.GetByteCount( paramName );

        var pData = Unsafe.AsPointer( ref data );
        var pName = stackalloc byte[nameByteCount + 1];

        Encoding.UTF8.GetBytes( paramName, new Span<byte>( pName, nameByteCount ) );

        CSoundOpGameSystem_SetSoundEventParamFunc.Invoke( pSoundOpGameSystem, pFilter, soundEventId, (nint)pName, (nint)pData, 0, 0 );
    }
}
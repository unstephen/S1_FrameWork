using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

/*! ZToolKit
\brief
  你懂得~
\author figo
\date   Jun, 19,2016
*/
#if ZTK
public unsafe class ZToolKit {
    static ZToolKit() {
        #if !UNITY_IPHONE
        F_LoadTextAsset( LoadTextAsset );
        #endif
    }
    public static System.Reflection.Assembly LoadAssembly( string path ) {
        System.Reflection.Assembly assembly = null;
        bool result = ReadDll( path, delegate( byte* data, int size) {
            byte[] buffer = new byte[size];
            Marshal.Copy( ( IntPtr )data, buffer, 0, size );
            assembly = System.Reflection.Assembly.Load( buffer );
        });

        if ( !result )
            return null;
        return assembly;
    }

    public static string LoadTextAsset( string path ) {
        try {
            UnityEngine.TextAsset asset = Resources.Load( path ) as UnityEngine.TextAsset;
            string text = asset.text;
            Resources.UnloadAsset( asset );
            return text;
        } catch ( Exception e ) {
            LOG.Debug( "**** {0} ", e.ToString() );
            return "";
        }
    }

    public static ushort BuildDogFlag( uint count, uint mask ) {
        return DogFlag( count, mask );
    }

    //>--------------------------------------------------------------------
#if UNITY_IPHONE
    const string LIBNAME = "__Internal";
#else
    const string LIBNAME = "ztoolkit";
#endif

    public unsafe delegate string D_LoadTextAsset( string path );
    public unsafe delegate void D_ReadDllCallBack( byte* data, int size );

    [DllImport( LIBNAME, EntryPoint = "F_LoadTextAsset", CallingConvention = CallingConvention.Cdecl )]
    private static extern void F_LoadTextAsset( D_LoadTextAsset f );

    [DllImport( LIBNAME, EntryPoint = "ReadDll", CallingConvention = CallingConvention.Cdecl )]
    private static extern bool ReadDll( string path, D_ReadDllCallBack cb );
    [DllImport( LIBNAME, EntryPoint = "DogFlag", CallingConvention = CallingConvention.Cdecl )]
    private static extern ushort DogFlag( uint count, uint mask );
}
#endif
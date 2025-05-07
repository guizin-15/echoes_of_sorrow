#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateCelesteStylePlayerData
{
    [MenuItem("Assets/Create/Player/Celeste Style Player Data")]
    public static void Create()
    {
        var data = ScriptableObject.CreateInstance<PlayerData>();

        // 1. Gravity
        data.fallGravityMult      = 1.5f;
        data.maxFallSpeed         = 25f;
        data.fastFallGravityMult  = 2f;
        data.maxFastFallSpeed     = 30f;

        // 2. Run
        data.runMaxSpeed      = 11f;
        data.runAcceleration  = 2.5f;
        data.runDecceleration = 5f;
        data.accelInAir       = 0.65f;
        data.deccelInAir      = 0.65f;
        data.doConserveMomentum = true;

        // 3. Jump
        data.jumpHeight           = 3.5f;
        data.jumpTimeToApex       = 0.3f;
        data.jumpCutGravityMult   = 2f;
        data.jumpHangGravityMult  = 0.5f;
        data.jumpHangTimeThreshold = 1f;
        data.jumpHangAccelerationMult = 1.1f;
        data.jumpHangMaxSpeedMult     = 1.3f;

        // 4. Wall Jump
        data.wallJumpForce    = new Vector2(15f, 25f);
        data.wallJumpRunLerp  = 0.5f;
        data.wallJumpTime     = 0.15f;
        data.doTurnOnWallJump = false;

        // 5. Slide
        data.slideSpeed = 0f;
        data.slideAccel = 0f;

        // 6. Assists
        data.coyoteTime          = 0.1f;
        data.jumpInputBufferTime = 0.1f;

        // 7. Dash
        data.dashAmount          = 1;
        data.dashSpeed           = 20f;
        data.dashSleepTime       = 0.05f;
        data.dashAttackTime      = 0.15f;
        data.dashEndTime         = 0.15f;
        data.dashEndSpeed        = new Vector2(15f, 15f);
        data.dashEndRunLerp      = 0.5f;
        data.dashRefillTime      = 0.1f;
        data.dashInputBufferTime = 0.1f;

        // deixa o OnValidate calcular campos derivados
        EditorUtility.SetDirty(data);

        AssetDatabase.CreateAsset(data, "Assets/Celeste Style Player Data.asset");
        AssetDatabase.SaveAssets();
        Selection.activeObject = data;
    }
}
#endif
using UnityEngine;

/// <summary>
/// 게임에서 사용되는 모든 상수를 관리하는 클래스
/// </summary>
public static class GameConstants
{
    // 게임 설정
    public const float DEFAULT_FALL_SPEED = 5f;
    public const float DEFAULT_MOVE_SPEED = 10f;
    public const float DEFAULT_SPAWN_DELAY = 0.5f;
    public const float DEFAULT_FLOATING_SPEED = 5f;
    public const float DEFAULT_GAME_START_DELAY = 5f;
    
    // UI 설정
    public const float HEART_FADE_DURATION = 0.5f;
    public const float HEART_DARK_COLOR = 0.25f;
    public const float PLAYER_MOVE_DURATION = 2f;
    
    // 오디오 설정
    public const float MIN_VOLUME_DB = -80f;
    public const float MAX_VOLUME_DB = 20f;
    
    // 태그 상수
    public const string REALITY_BLOCK_TAG = "RealityBlock";
    public const string DREAM_BLOCK_TAG = "DreamBlock";
    
    // 레벨 설정
    public const int DEFAULT_LEVEL = 1;
    public const int LEVEL_UP_OFFSET = 30;
    public const float LEVEL_UP_DURATION = 3f;
    
    // 인트로 설정
    public const float INTRO_DURATION = 9f;
    public const float INTRO_SKIP_HOLD_TIME = 1f;
    public const float INTRO_EASE_DURATION = 1f;
    
    // 블록 설정
    public const float BLOCK_ROTATION_SPEED = 180f;
    public const float BLOCK_FAST_FALL_MULTIPLIER = 2f;
    
    // 카메라 설정
    public const float CAMERA_FIELD_OF_VIEW = 60f;
    public const float CAMERA_ORTHOGRAPHIC_SIZE = 10f;
    
    // 물리 설정
    public const float DEFAULT_GRAVITY_SCALE = 1f;
    public const float ZERO_GRAVITY_SCALE = 0f;
    
    // 애니메이션 설정
    public const float BLOCK_FADE_DURATION = 0.5f;
    public const float UI_ANIMATION_DURATION = 1f;
    
    // 성능 설정
    public const int MAX_BLOCK_COUNT = 1000;
    public const float MAX_HEIGHT_UPDATE_THRESHOLD = 0.1f;
} 
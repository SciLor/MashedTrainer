/*
 * MFL_Dummy - Mock Mashed (MFL.exe) process for testing SciLors Mashed Trainer
 *
 * Implements the memory layout documented in doc/TrainerAnalysis.md:
 * - Player car structs at 0x8B06E0 (stride 0xD04)
 * - Weapon slots at 0x8BED20 (stride 0xB4)
 * - Points at 0x8D8B40, visual 0x8D8B60, change 0x8D8B80
 * - Player count at 0x8D8B30
 * - Distances at 0x8C7E40
 * - Damage table at 0x65A9E8 (stride 0x28)
 * - Maximum points at 0x658DE4
 * - Game active flag at 0x6AE110
 */

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdint.h>
#include <stdbool.h>
#include <math.h>

#ifdef _WIN32
#include <windows.h>
#define SLEEP_MS(ms) Sleep(ms)
#else
#include <unistd.h>
#define SLEEP_MS(ms) usleep((ms) * 1000)
#endif

#define PROCESS_BASE 0x400000
#define MEM_SIZE     0x600000 /* 6 MB buffer covering 0x400000 .. 0xA00000 */

/* Addresses */
#define ADDR_PLAYER_COUNT        0x8D8B30
#define ADDR_MAX_POINTS          0x658DE4
#define ADDR_GAME_ACTIVE         0x6AE110
#define ADDR_BASE_PLAYER         0x8B06E0
#define ADDR_BASE_WEAPON         0x8BEDCC
#define ADDR_BASE_POINTS         0x8D8B40
#define ADDR_BASE_POINTS_CHANGE  0x8D8B80
#define ADDR_BASE_POINTS_VISUAL  0x8D8B60
#define ADDR_BASE_DISTANCE       0x8C7E40
#define ADDR_BASE_DAMAGE         0x65A9E8
#define ADDR_BASE_COLOR          0x69D028

#define PLAYER_STRIDE            0xD04
#define DAMAGE_STRIDE            0x28
#define WEAPON_STRIDE            0xB4

/* Internal memory buffer */
static uint8_t *g_memory = NULL;

static inline void *mem_ptr(uint32_t addr) {
    if (addr < PROCESS_BASE || addr >= (PROCESS_BASE + MEM_SIZE)) {
        return NULL;
    }
    return &g_memory[addr - PROCESS_BASE];
}

static inline uint32_t read_u32(uint32_t addr) {
    uint32_t *p = (uint32_t *)mem_ptr(addr);
    return p ? *p : 0;
}

static inline void write_u32(uint32_t addr, uint32_t val) {
    uint32_t *p = (uint32_t *)mem_ptr(addr);
    if (p) *p = val;
}

static inline float read_f32(uint32_t addr) {
    float *p = (float *)mem_ptr(addr);
    return p ? *p : 0.0f;
}

static inline void write_f32(uint32_t addr, float val) {
    float *p = (float *)mem_ptr(addr);
    if (p) *p = val;
}

void init_dummy_memory(void) {
    g_memory = (uint8_t *)calloc(1, MEM_SIZE);
    if (!g_memory) {
        fprintf(stderr, "Failed to allocate dummy game memory buffer!\n");
        exit(1);
    }

    /* Game state */
    write_u32(ADDR_PLAYER_COUNT, 4);
    write_u32(ADDR_MAX_POINTS, 8);
    write_u32(ADDR_GAME_ACTIVE, 1);

    /* Initialize 4 players */
    for (int i = 0; i < 4; i++) {
        uint32_t p_addr = ADDR_BASE_PLAYER + (i * PLAYER_STRIDE);
        uint32_t dmg_addr = ADDR_BASE_DAMAGE + (i * DAMAGE_STRIDE);
        uint32_t pts_addr = ADDR_BASE_POINTS + (i * 4);

        /* Player Alive: 1 */
        write_u32(p_addr + 0x004, 1);
        /* Controls Disabled: 0 */
        write_u32(p_addr + 0x010, 0);
        /* Bot: player 0 is human, 1..3 are bots */
        write_u32(p_addr + 0xD00, i > 0 ? 1 : 0);

        /* Points */
        write_u32(pts_addr, 2 + i);

        /* Matrix double buffer index: 0 */
        write_u32(p_addr + 0x9AC, 0);

        /* Pose matrix in buffer 0 (0x928) and buffer 1 (0x968) */
        for (int buf = 0; buf < 2; buf++) {
            uint32_t m_addr = p_addr + (buf == 0 ? 0x928 : 0x968);
            /* Right vector: (1, 0, 0) */
            write_f32(m_addr + 0x00, 1.0f);
            write_f32(m_addr + 0x04, 0.0f);
            write_f32(m_addr + 0x08, 0.0f);
            /* Up vector: (0, 1, 0) */
            write_f32(m_addr + 0x10, 0.0f);
            write_f32(m_addr + 0x14, 1.0f);
            write_f32(m_addr + 0x18, 0.0f);
            /* At vector: (0, 0, 1) */
            write_f32(m_addr + 0x20, 0.0f);
            write_f32(m_addr + 0x24, 0.0f);
            write_f32(m_addr + 0x28, 1.0f);
            /* Pos vector: (X, Y_height, Z) */
            write_f32(m_addr + 0x30, 10.0f * (i + 1)); /* X */
            write_f32(m_addr + 0x34, 1.0f);            /* Y (RenderWare height) */
            write_f32(m_addr + 0x38, 20.0f * (i + 1)); /* Z */
        }

        /* Velocity: (0, 0, 0) */
        write_f32(p_addr + 0x144, 0.0f);
        write_f32(p_addr + 0x148, 0.0f);
        write_f32(p_addr + 0x14C, 0.0f);

        /* Distance */
        write_f32(ADDR_BASE_DISTANCE + (i * 4), 5.0f * i);

        /* Damage: Front 0%, Back 0% */
        write_f32(dmg_addr + 0x00, 0.0f);
        write_f32(dmg_addr + 0x04, 0.0f);
    }

    printf("[MFL_Dummy] Game memory initialized (Base: 0x%X, Size: %u KB)\n",
           PROCESS_BASE, MEM_SIZE / 1024);
    printf("[MFL_Dummy] 4 Players ready, Game Active: 1, Max Points: 8\n");
}

int main(int argc, char **argv) {
    printf("==========================================\n");
    printf("  MFL_Dummy - Mashed Game Simulator\n");
    printf("==========================================\n");

    init_dummy_memory();

    bool run_once = false;
    if (argc > 1 && strcmp(argv[1], "--once") == 0) {
        run_once = true;
    }

    printf("[MFL_Dummy] Running simulation loop (Ctrl+C to stop)...\n");

    uint32_t tick = 0;
    while (1) {
        tick++;

        /* Check if player 0 was flipped (up.y < 0) */
        uint32_t p0 = ADDR_BASE_PLAYER;
        float up_y = read_f32(p0 + 0x928 + 0x14);
        float pos_x = read_f32(p0 + 0x928 + 0x30);
        uint32_t pts0 = read_u32(ADDR_BASE_POINTS);

        if (tick % 20 == 0) {
            printf("[Tick %u] P1: Points=%u, PosX=%.2f, UpY=%.2f (%s)\n",
                   tick, pts0, pos_x, up_y, (up_y < 0.0f) ? "ON ROOF" : "UPRIGHT");
        }

        if (run_once && tick >= 5) {
            break;
        }

        SLEEP_MS(50);
    }

    free(g_memory);
    printf("[MFL_Dummy] Clean exit.\n");
    return 0;
}


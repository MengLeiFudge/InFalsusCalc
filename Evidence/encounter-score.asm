
FUNCTION 9530 BattleScore get_ReductionFactor 0x180489c90 48
180489c90 cmp      dword ptr [rcx + 0x34], 0
180489c94 jle      0x180489cad
180489c96 movd     xmm0, dword ptr [rcx + 0x30]
180489c9b movd     xmm1, dword ptr [rcx + 0x34]
180489ca0 cvtdq2pd xmm0, xmm0
180489ca4 cvtdq2pd xmm1, xmm1
180489ca8 divsd    xmm0, xmm1
180489cac ret      
180489cad movsd    xmm0, qword ptr [rip + 0x217e4ab]
180489cb5 ret      

FUNCTION 9530 BattleScore get_TotalBattleScore 0x180489cc0 160
180489cc0 push     rbx
180489cc2 sub      rsp, 0x30
180489cc6 cmp      byte ptr [rip + 0x2eb385c], 0
180489ccd mov      rbx, rcx
180489cd0 movaps   xmmword ptr [rsp + 0x20], xmm6
180489cd5 jne      0x180489cea
180489cd7 lea      rcx, [rip + 0x2cf9622]
180489cde call     0x180309af0 ; 
180489ce3 mov      byte ptr [rip + 0x2eb383f], 1
180489cea mov      rcx, qword ptr [rip + 0x2cf960f]
180489cf1 cmp      dword ptr [rcx + 0xe4], 0
180489cf8 jne      0x180489cff
180489cfa call     0x180309de0 ; 
180489cff xor      edx, edx
180489d01 mov      rcx, rbx
180489d04 call     0x180489ba0 ; 9530:ifapp.Game.Data.BattleScore.get_OffensiveBattleScore
180489d09 xor      edx, edx
180489d0b mov      rcx, rbx
180489d0e movaps   xmm6, xmm0
180489d11 call     0x180489ac0 ; 9530:ifapp.Game.Data.BattleScore.get_DefensiveBattleScore
180489d16 cmp      dword ptr [rbx + 0x34], 0
180489d1a jle      0x180489d34
180489d1c movd     xmm2, dword ptr [rbx + 0x30]
180489d21 movd     xmm1, dword ptr [rbx + 0x34]
180489d26 cvtdq2pd xmm2, xmm2
180489d2a cvtdq2pd xmm1, xmm1
180489d2e divsd    xmm2, xmm1
180489d32 jmp      0x180489d3c ; 
180489d34 movsd    xmm2, qword ptr [rip + 0x217e424]
180489d3c mulsd    xmm0, xmm6
180489d40 movaps   xmm6, xmmword ptr [rsp + 0x20]
180489d45 divsd    xmm0, qword ptr [rip + 0x217e423]
180489d4d mulsd    xmm0, xmm2
180489d51 add      rsp, 0x30
180489d55 pop      rbx
180489d56 ret      

FUNCTION 9530 BattleScore get_OffensiveBattleScore 0x180489ba0 240
180489ba0 push     rbx
180489ba2 sub      rsp, 0x50
180489ba6 cmp      byte ptr [rip + 0x2eb397d], 0
180489bad mov      rbx, rcx
180489bb0 movaps   xmmword ptr [rsp + 0x40], xmm6
180489bb5 movaps   xmmword ptr [rsp + 0x30], xmm7
180489bba movaps   xmmword ptr [rsp + 0x20], xmm8
180489bc0 jne      0x180489be1
180489bc2 lea      rcx, [rip + 0x2cf9737]
180489bc9 call     0x180309af0 ; 
180489bce lea      rcx, [rip + 0x2d3caeb]
180489bd5 call     0x180309af0 ; 
180489bda mov      byte ptr [rip + 0x2eb3949], 1
180489be1 mov      rcx, qword ptr [rip + 0x2d3cad8]
180489be8 movsd    xmm7, qword ptr [rbx + 0x28]
180489bed movsd    xmm6, qword ptr [rbx]
180489bf1 cmp      dword ptr [rcx + 0xe4], 0
180489bf8 jne      0x180489bff
180489bfa call     0x180309de0 ; 
180489bff movsd    xmm0, qword ptr [rip + 0x217e939]
180489c07 xor      r8d, r8d
180489c0a movaps   xmm1, xmm6
180489c0d call     0x1818f7010 ; 266:System.Math.Max
180489c12 mov      rcx, qword ptr [rip + 0x2cf96e7]
180489c19 movaps   xmm6, xmm0
180489c1c movsd    xmm8, qword ptr [rbx + 0x18]
180489c22 cmp      dword ptr [rcx + 0xe4], 0
180489c29 jne      0x180489c30
180489c2b call     0x180309de0 ; 
180489c30 cmp      dword ptr [rbx + 0x34], 0
180489c34 jle      0x180489c4e
180489c36 movd     xmm1, dword ptr [rbx + 0x30]
180489c3b movd     xmm0, dword ptr [rbx + 0x34]
180489c40 cvtdq2pd xmm1, xmm1
180489c44 cvtdq2pd xmm0, xmm0
180489c48 divsd    xmm1, xmm0
180489c4c jmp      0x180489c56 ; 
180489c4e movsd    xmm1, qword ptr [rip + 0x217e50a]
180489c56 addsd    xmm8, qword ptr [rip + 0x217e8f9]
180489c5f mulsd    xmm7, qword ptr [rip + 0x217e509]
180489c67 divsd    xmm6, xmm8
180489c6c movaps   xmm8, xmmword ptr [rsp + 0x20]
180489c72 divsd    xmm6, xmm1
180489c76 mulsd    xmm6, xmm7
180489c7a movaps   xmm7, xmmword ptr [rsp + 0x30]
180489c7f movaps   xmm0, xmm6
180489c82 movaps   xmm6, xmmword ptr [rsp + 0x40]
180489c87 add      rsp, 0x50
180489c8b pop      rbx
180489c8c ret      

FUNCTION 9530 BattleScore get_DefensiveBattleScore 0x180489ac0 224
180489ac0 push     rbx
180489ac2 sub      rsp, 0x40
180489ac6 cmp      byte ptr [rip + 0x2eb3a5e], 0
180489acd mov      rbx, rcx
180489ad0 movaps   xmmword ptr [rsp + 0x30], xmm6
180489ad5 movaps   xmmword ptr [rsp + 0x20], xmm7
180489ada jne      0x180489afb
180489adc lea      rcx, [rip + 0x2cf981d]
180489ae3 call     0x180309af0 ; 
180489ae8 lea      rcx, [rip + 0x2d3cbd1]
180489aef call     0x180309af0 ; 
180489af4 mov      byte ptr [rip + 0x2eb3a30], 1
180489afb mov      rcx, qword ptr [rip + 0x2d3cbbe]
180489b02 movsd    xmm6, qword ptr [rbx + 8]
180489b07 cmp      dword ptr [rcx + 0xe4], 0
180489b0e jne      0x180489b15
180489b10 call     0x180309de0 ; 
180489b15 movsd    xmm0, qword ptr [rip + 0x217ea23]
180489b1d xor      r8d, r8d
180489b20 movaps   xmm1, xmm6
180489b23 call     0x1818f7010 ; 266:System.Math.Max
180489b28 mov      rcx, qword ptr [rip + 0x2cf97d1]
180489b2f movaps   xmm7, xmm0
180489b32 movsd    xmm6, qword ptr [rbx + 0x10]
180489b37 cmp      dword ptr [rcx + 0xe4], 0
180489b3e jne      0x180489b45
180489b40 call     0x180309de0 ; 
180489b45 cmp      dword ptr [rbx + 0x34], 0
180489b49 jle      0x180489b63
180489b4b movd     xmm1, dword ptr [rbx + 0x30]
180489b50 movd     xmm0, dword ptr [rbx + 0x34]
180489b55 cvtdq2pd xmm1, xmm1
180489b59 cvtdq2pd xmm0, xmm0
180489b5d divsd    xmm1, xmm0
180489b61 jmp      0x180489b6b ; 
180489b63 movsd    xmm1, qword ptr [rip + 0x217e5f5]
180489b6b addsd    xmm6, qword ptr [rip + 0x217e9e5]
180489b73 movsd    xmm0, qword ptr [rip + 0x217e5f5]
180489b7b divsd    xmm7, xmm6
180489b7f movaps   xmm6, xmmword ptr [rsp + 0x30]
180489b84 divsd    xmm0, xmm7
180489b88 movaps   xmm7, xmmword ptr [rsp + 0x20]
180489b8d mulsd    xmm0, xmm1
180489b91 add      rsp, 0x40
180489b95 pop      rbx
180489b96 ret      

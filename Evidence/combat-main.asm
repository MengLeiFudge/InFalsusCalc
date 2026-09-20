
FUNCTION 9636 _Vh _uMA 0x1804b9ea0 32
1804b9ea0 xorps    xmm4, xmm4
1804b9ea3 ucomisd  xmm1, xmm4
1804b9ea7 jp       0x1804b9eb3
1804b9ea9 jne      0x1804b9eb3
1804b9eab movsd    xmm1, qword ptr [rip + 0x214e6a5]
1804b9eb3 divsd    xmm0, xmm1
1804b9eb7 mulsd    xmm0, xmm2
1804b9ebb mulsd    xmm0, xmm3
1804b9ebf ret      

FUNCTION 9636 _Vh _vMA 0x1804b9ec0 1520
1804b9ec0 mov      qword ptr [rsp + 0x18], rbx
1804b9ec5 push     rsi
1804b9ec6 push     rdi
1804b9ec7 push     r14
1804b9ec9 sub      rsp, 0x90
1804b9ed0 cmp      byte ptr [rip + 0x2e83728], 0
1804b9ed7 mov      esi, r9d
1804b9eda movzx    r14d, r8b
1804b9ede mov      rdi, rdx
1804b9ee1 mov      rbx, rcx
1804b9ee4 jne      0x1804b9f05
1804b9ee6 lea      rcx, [rip + 0x2cbbc4b]
1804b9eed call     0x180309af0 ; 
1804b9ef2 lea      rcx, [rip + 0x2cf54cf]
1804b9ef9 call     0x180309af0 ; 
1804b9efe mov      byte ptr [rip + 0x2e836fa], 1
1804b9f05 mov      qword ptr [rsp + 0xb0], rbp
1804b9f0d mov      qword ptr [rsp + 0xb8], r15
1804b9f15 movaps   xmmword ptr [rsp + 0x80], xmm6
1804b9f1d movaps   xmmword ptr [rsp + 0x70], xmm7
1804b9f22 movaps   xmmword ptr [rsp + 0x60], xmm8
1804b9f28 movaps   xmmword ptr [rsp + 0x50], xmm9
1804b9f2e movaps   xmmword ptr [rsp + 0x40], xmm10
1804b9f34 movaps   xmmword ptr [rsp + 0x30], xmm11
1804b9f3a movaps   xmmword ptr [rsp + 0x20], xmm12
1804b9f40 test     rbx, rbx
1804b9f43 je       0x1804ba496
1804b9f49 mov      eax, esi
1804b9f4b shl      eax, 0xd
1804b9f4e xor      eax, esi
1804b9f50 mov      esi, dword ptr [rsp + 0xd0]
1804b9f57 mov      ecx, eax
1804b9f59 shr      ecx, 0x11
1804b9f5c xor      ecx, eax
1804b9f5e mov      eax, ecx
1804b9f60 shl      eax, 5
1804b9f63 xor      eax, ecx
1804b9f65 mov      dword ptr [rbx + 0x28], eax
1804b9f68 mov      eax, dword ptr [rsp + 0xd8]
1804b9f6f mov      dword ptr [rbx + 0x1f0], eax
1804b9f75 mov      dword ptr [rbx + 0x1c8], esi
1804b9f7b mov      byte ptr [rbx + 0x2c], r14b
1804b9f7f test     rdi, rdi
1804b9f82 je       0x1804ba496
1804b9f88 mov      rdx, qword ptr [rip + 0x2cbbba9]
1804b9f8f mov      rcx, rdi
1804b9f92 call     0x180fa0d20 ; 
1804b9f97 movsd    xmm12, qword ptr [rip + 0x214e5b8]
1804b9fa0 movabs   r15, 0x4059000000000000
1804b9faa cmp      esi, 5
1804b9fad jl       0x1804ba195
1804b9fb3 xor      edi, edi
1804b9fb5 mov      ecx, edi
1804b9fb7 nop      word ptr [rax + rax]
1804b9fc0 mov      rdx, qword ptr [rbx + 0x238]
1804b9fc7 test     rdx, rdx
1804b9fca je       0x1804ba496
1804b9fd0 cmp      ecx, dword ptr [rdx + 0x18]
1804b9fd3 jae      0x1804ba49c
1804b9fd9 mov      eax, ecx
1804b9fdb mov      dword ptr [rdx + rax*4 + 0x20], edi
1804b9fdf mov      rdx, qword ptr [rbx + 0x240]
1804b9fe6 test     rdx, rdx
1804b9fe9 je       0x1804ba496
1804b9fef cmp      ecx, dword ptr [rdx + 0x18]
1804b9ff2 jae      0x1804ba49c
1804b9ff8 mov      eax, ecx
1804b9ffa mov      dword ptr [rdx + rax*4 + 0x20], edi
1804b9ffe mov      rdx, qword ptr [rbx + 0x248]
1804ba005 test     rdx, rdx
1804ba008 je       0x1804ba496
1804ba00e cmp      ecx, dword ptr [rdx + 0x18]
1804ba011 jae      0x1804ba49c
1804ba017 mov      eax, ecx
1804ba019 inc      ecx
1804ba01b mov      qword ptr [rdx + rax*8 + 0x20], rdi
1804ba020 cmp      ecx, 5
1804ba023 jl       0x1804b9fc0
1804ba025 movsd    xmm11, qword ptr [rip + 0x214e57a]
1804ba02e movaps   xmm10, xmm12
1804ba032 movd     xmm0, esi
1804ba036 mov      ebp, edi
1804ba038 movd     xmm7, esi
1804ba03c xorps    xmm8, xmm8
1804ba040 cvtdq2pd xmm0, xmm0
1804ba044 mov      esi, edi
1804ba046 cvtdq2pd xmm7, xmm7
1804ba04a divsd    xmm10, xmm0
1804ba04f nop      
1804ba050 mov      rcx, qword ptr [rbx + 0x238]
1804ba057 test     rcx, rcx
1804ba05a je       0x1804ba496
1804ba060 cmp      esi, dword ptr [rcx + 0x18]
1804ba063 jae      0x1804ba49c
1804ba069 movaps   xmm6, xmm7
1804ba06c movd     xmm0, esi
1804ba070 cvtdq2pd xmm0, xmm0
1804ba074 mov      eax, esi
1804ba076 mov      dword ptr [rcx + rax*4 + 0x20], ebp
1804ba07a cmp      byte ptr [rip + 0x2e83407], dil
1804ba081 movaps   xmm1, xmm11
1804ba085 subsd    xmm1, xmm0
1804ba089 xorps    xmm9, xmm9
1804ba08d divsd    xmm6, xmm1
1804ba091 movaps   xmm0, xmm6
1804ba094 addsd    xmm0, xmm8
1804ba099 cvtsd2ss xmm9, xmm0
1804ba09e jne      0x1804ba0b3
1804ba0a0 lea      rcx, [rip + 0x2d0c619]
1804ba0a7 call     0x180309af0 ; 
1804ba0ac mov      byte ptr [rip + 0x2e833d5], 1
1804ba0b3 mov      rcx, qword ptr [rip + 0x2d0c606]
1804ba0ba cmp      dword ptr [rcx + 0xe4], edi
1804ba0c0 jne      0x1804ba0c7
1804ba0c2 call     0x180309de0 ; 
1804ba0c7 cvtps2pd xmm0, xmm9
1804ba0cb call     0x1800099f0 ; 
1804ba0d0 mov      rcx, qword ptr [rbx + 0x240]
1804ba0d7 movaps   xmm2, xmm6
1804ba0da cvttsd2si edx, xmm0
1804ba0de movd     xmm1, edx
1804ba0e2 cvtdq2pd xmm1, xmm1
1804ba0e6 subsd    xmm2, xmm1
1804ba0ea test     rcx, rcx
1804ba0ed je       0x1804ba496
1804ba0f3 cmp      esi, dword ptr [rcx + 0x18]
1804ba0f6 jae      0x1804ba49c
1804ba0fc mov      eax, esi
1804ba0fe mov      dword ptr [rcx + rax*4 + 0x20], edx
1804ba102 mov      rcx, qword ptr [rbx + 0x248]
1804ba109 test     rcx, rcx
1804ba10c je       0x1804ba496
1804ba112 cmp      esi, dword ptr [rcx + 0x18]
1804ba115 jae      0x1804ba49c
1804ba11b mov      eax, esi
1804ba11d addsd    xmm8, xmm2
1804ba122 add      ebp, edx
1804ba124 subsd    xmm7, xmm6
1804ba128 inc      esi
1804ba12a movsd    qword ptr [rcx + rax*8 + 0x20], xmm10
1804ba131 cmp      esi, 5
1804ba134 jl       0x1804ba050
1804ba13a xorps    xmm0, xmm0
1804ba13d lea      rcx, [rbx + 0x218]
1804ba144 movups   xmmword ptr [rbx + 0x218], xmm0
1804ba14b xor      r8d, r8d
1804ba14e movups   xmmword ptr [rbx + 0x226], xmm0
1804ba155 mov      rdx, qword ptr [rbx + 0x240]
1804ba15c call     0x1804b76a0 ; 9708:_K._DH._RnA
1804ba161 mov      rax, qword ptr [rbx + 0x240]
1804ba168 nop      dword ptr [rax + rax]
1804ba170 test     rax, rax
1804ba173 je       0x1804ba496
1804ba179 nop      dword ptr [rax]
1804ba180 cmp      edi, dword ptr [rax + 0x18]
1804ba183 jae      0x1804ba49c
1804ba189 inc      edi
1804ba18b cmp      edi, 5
1804ba18e jl       0x1804ba180
1804ba190 jmp      0x1804ba34d ; 
1804ba195 mov      rax, qword ptr [rbx + 0x238]
1804ba19c test     rax, rax
1804ba19f je       0x1804ba496
1804ba1a5 cmp      dword ptr [rax + 0x18], 0
1804ba1a9 jbe      0x1804ba49c
1804ba1af xor      edi, edi
1804ba1b1 mov      dword ptr [rax + 0x20], edi
1804ba1b4 mov      rax, qword ptr [rbx + 0x238]
1804ba1bb test     rax, rax
1804ba1be je       0x1804ba496
1804ba1c4 cmp      dword ptr [rax + 0x18], 1
1804ba1c8 jbe      0x1804ba49c
1804ba1ce mov      dword ptr [rax + 0x24], edi
1804ba1d1 mov      rax, qword ptr [rbx + 0x238]
1804ba1d8 test     rax, rax
1804ba1db je       0x1804ba496
1804ba1e1 cmp      dword ptr [rax + 0x18], 2
1804ba1e5 jbe      0x1804ba49c
1804ba1eb mov      dword ptr [rax + 0x28], edi
1804ba1ee mov      rax, qword ptr [rbx + 0x238]
1804ba1f5 test     rax, rax
1804ba1f8 je       0x1804ba496
1804ba1fe cmp      dword ptr [rax + 0x18], 3
1804ba202 jbe      0x1804ba49c
1804ba208 mov      dword ptr [rax + 0x2c], edi
1804ba20b mov      rax, qword ptr [rbx + 0x238]
1804ba212 test     rax, rax
1804ba215 je       0x1804ba496
1804ba21b cmp      dword ptr [rax + 0x18], 4
1804ba21f jbe      0x1804ba49c
1804ba225 mov      dword ptr [rax + 0x30], esi
1804ba228 mov      rax, qword ptr [rbx + 0x240]
1804ba22f test     rax, rax
1804ba232 je       0x1804ba496
1804ba238 cmp      dword ptr [rax + 0x18], edi
1804ba23b jbe      0x1804ba49c
1804ba241 mov      dword ptr [rax + 0x20], edi
1804ba244 mov      rax, qword ptr [rbx + 0x240]
1804ba24b test     rax, rax
1804ba24e je       0x1804ba496
1804ba254 cmp      dword ptr [rax + 0x18], 1
1804ba258 jbe      0x1804ba49c
1804ba25e mov      dword ptr [rax + 0x24], edi
1804ba261 mov      rax, qword ptr [rbx + 0x240]
1804ba268 test     rax, rax
1804ba26b je       0x1804ba496
1804ba271 cmp      dword ptr [rax + 0x18], 2
1804ba275 jbe      0x1804ba49c
1804ba27b mov      dword ptr [rax + 0x28], edi
1804ba27e mov      rax, qword ptr [rbx + 0x240]
1804ba285 test     rax, rax
1804ba288 je       0x1804ba496
1804ba28e cmp      dword ptr [rax + 0x18], 3
1804ba292 jbe      0x1804ba49c
1804ba298 mov      dword ptr [rax + 0x2c], edi
1804ba29b mov      rax, qword ptr [rbx + 0x240]
1804ba2a2 test     rax, rax
1804ba2a5 je       0x1804ba496
1804ba2ab cmp      dword ptr [rax + 0x18], 4
1804ba2af jbe      0x1804ba49c
1804ba2b5 mov      dword ptr [rax + 0x30], esi
1804ba2b8 mov      rax, qword ptr [rbx + 0x248]
1804ba2bf test     rax, rax
1804ba2c2 je       0x1804ba496
1804ba2c8 cmp      dword ptr [rax + 0x18], edi
1804ba2cb jbe      0x1804ba49c
1804ba2d1 mov      qword ptr [rax + 0x20], r15
1804ba2d5 mov      rax, qword ptr [rbx + 0x248]
1804ba2dc test     rax, rax
1804ba2df je       0x1804ba496
1804ba2e5 cmp      dword ptr [rax + 0x18], 1
1804ba2e9 jbe      0x1804ba49c
1804ba2ef mov      qword ptr [rax + 0x28], r15
1804ba2f3 mov      rax, qword ptr [rbx + 0x248]
1804ba2fa test     rax, rax
1804ba2fd je       0x1804ba496
1804ba303 cmp      dword ptr [rax + 0x18], 2
1804ba307 jbe      0x1804ba49c
1804ba30d mov      qword ptr [rax + 0x30], r15
1804ba311 mov      rax, qword ptr [rbx + 0x248]
1804ba318 test     rax, rax
1804ba31b je       0x1804ba496
1804ba321 cmp      dword ptr [rax + 0x18], 3
1804ba325 jbe      0x1804ba49c
1804ba32b mov      qword ptr [rax + 0x38], r15
1804ba32f mov      rax, qword ptr [rbx + 0x248]
1804ba336 test     rax, rax
1804ba339 je       0x1804ba496
1804ba33f cmp      dword ptr [rax + 0x18], 4
1804ba343 jbe      0x1804ba49c
1804ba349 mov      qword ptr [rax + 0x40], r15
1804ba34d cmp      byte ptr [rbx + 0x26], 0
1804ba351 je       0x1804ba35c
1804ba353 movsd    xmm12, qword ptr [rip + 0x214e25c]
1804ba35c cmp      r14b, 1
1804ba360 je       0x1804ba3df
1804ba362 cmp      r14b, 2
1804ba366 je       0x1804ba3aa
1804ba368 cmp      r14b, 3
1804ba36c jne      0x1804ba431
1804ba372 movsd    qword ptr [rbx + 0x280], xmm12
1804ba37b mov      qword ptr [rbx + 0x290], r15
1804ba382 mov      rax, qword ptr [rbx + 0x280]
1804ba389 mov      qword ptr [rbx + 0x278], rax
1804ba390 mov      rax, qword ptr [rbx + 0x290]
1804ba397 mov      qword ptr [rbx + 0x288], rax
1804ba39e mov      byte ptr [rbx + 0x298], 0
1804ba3a5 jmp      0x1804ba431 ; 
1804ba3aa movsd    qword ptr [rbx + 0x258], xmm12
1804ba3b3 mov      qword ptr [rbx + 0x268], r15
1804ba3ba mov      rax, qword ptr [rbx + 0x258]
1804ba3c1 mov      qword ptr [rbx + 0x250], rax
1804ba3c8 mov      rax, qword ptr [rbx + 0x268]
1804ba3cf mov      qword ptr [rbx + 0x260], rax
1804ba3d6 mov      byte ptr [rbx + 0x270], 0
1804ba3dd jmp      0x1804ba412 ; 
1804ba3df movsd    qword ptr [rbx + 0x280], xmm12
1804ba3e8 mov      qword ptr [rbx + 0x290], r15
1804ba3ef mov      rax, qword ptr [rbx + 0x280]
1804ba3f6 mov      qword ptr [rbx + 0x278], rax
1804ba3fd mov      rax, qword ptr [rbx + 0x290]
1804ba404 mov      qword ptr [rbx + 0x288], rax
1804ba40b mov      byte ptr [rbx + 0x298], 0
1804ba412 mov      rcx, qword ptr [rip + 0x2cf4faf]
1804ba419 cmp      dword ptr [rcx + 0xe4], 0
1804ba420 jne      0x1804ba427
1804ba422 call     0x180309de0 ; 
1804ba427 xor      edx, edx
1804ba429 mov      rcx, rbx
1804ba42c call     0x1804b9030 ; 9636:_K._Vh._XMA
1804ba431 mov      rcx, qword ptr [rbx + 0x2a0]
1804ba438 test     rcx, rcx
1804ba43b je       0x1804ba496
1804ba43d xor      r8d, r8d
1804ba440 mov      rdx, rbx
1804ba443 movaps   xmm12, xmmword ptr [rsp + 0x20]
1804ba449 movaps   xmm11, xmmword ptr [rsp + 0x30]
1804ba44f movaps   xmm10, xmmword ptr [rsp + 0x40]
1804ba455 movaps   xmm9, xmmword ptr [rsp + 0x50]
1804ba45b movaps   xmm8, xmmword ptr [rsp + 0x60]
1804ba461 movaps   xmm7, xmmword ptr [rsp + 0x70]
1804ba466 movaps   xmm6, xmmword ptr [rsp + 0x80]
1804ba46e mov      r15, qword ptr [rsp + 0xb8]
1804ba476 mov      rbp, qword ptr [rsp + 0xb0]
1804ba47e mov      rbx, qword ptr [rsp + 0xc0]
1804ba486 add      rsp, 0x90
1804ba48d pop      r14
1804ba48f pop      rdi
1804ba490 pop      rsi
1804ba491 jmp      0x1804953e0 ; 9631:ifapp.Game.Data.GameplayHistory._ymA
1804ba496 call     0x180309d40 ; 
1804ba49c call     0x180309d30 ; 

FUNCTION 9636 _Vh _VMA 0x1804b83a0 64
1804b83a0 cmp      cl, 4
1804b83a3 jb       0x1804b83ab
1804b83a5 cmp      r8b, 1
1804b83a9 je       0x1804b83b4
1804b83ab test     dl, dl
1804b83ad je       0x1804b83ba
1804b83af cmp      cl, 4
1804b83b2 jb       0x1804b83ba
1804b83b4 mov      eax, 3
1804b83b9 ret      
1804b83ba movzx    eax, cl
1804b83bd sub      eax, 2
1804b83c0 cmp      eax, 1
1804b83c3 ja       0x1804b83cb
1804b83c5 mov      eax, 1
1804b83ca ret      
1804b83cb mov      eax, 2
1804b83d0 movzx    ecx, cl
1804b83d3 xor      edx, edx
1804b83d5 sub      ecx, 4
1804b83d8 cmp      ecx, eax
1804b83da cmova    eax, edx
1804b83dd ret      

FUNCTION 9636 _Vh _wMA 0x1804ba4b0 112
1804ba4b0 push     rbx
1804ba4b2 sub      rsp, 0x20
1804ba4b6 cmp      byte ptr [rip + 0x2e83143], 0
1804ba4bd movsxd   rbx, ecx
1804ba4c0 jne      0x1804ba4d5
1804ba4c2 lea      rcx, [rip + 0x2cf4eff]
1804ba4c9 call     0x180309af0 ; 
1804ba4ce mov      byte ptr [rip + 0x2e8312b], 1
1804ba4d5 mov      rax, qword ptr [rip + 0x2cf4eec]
1804ba4dc cmp      dword ptr [rax + 0xe4], 0
1804ba4e3 jne      0x1804ba4f4
1804ba4e5 mov      rcx, rax
1804ba4e8 call     0x180309de0 ; 
1804ba4ed mov      rax, qword ptr [rip + 0x2cf4ed4]
1804ba4f4 mov      rax, qword ptr [rax + 0xb8]
1804ba4fb mov      rcx, qword ptr [rax]
1804ba4fe test     rcx, rcx
1804ba501 je       0x1804ba514
1804ba503 cmp      ebx, dword ptr [rcx + 0x18]
1804ba506 jae      0x1804ba51a
1804ba508 movsd    xmm0, qword ptr [rcx + rbx*8 + 0x20]
1804ba50e add      rsp, 0x20
1804ba512 pop      rbx
1804ba513 ret      
1804ba514 call     0x180309d40 ; 
1804ba51a call     0x180309d30 ; 

FUNCTION 9636 _Vh _WMA 0x1804b83e0 3152
1804b83e0 mov      qword ptr [rsp + 0x20], r9
1804b83e5 mov      qword ptr [rsp + 0x10], rdx
1804b83ea mov      qword ptr [rsp + 8], rcx
1804b83ef push     rbp
1804b83f0 push     rbx
1804b83f1 push     rdi
1804b83f2 lea      rbp, [rsp - 0x80]
1804b83f7 sub      rsp, 0x180
1804b83fe cmp      byte ptr [rip + 0x2e851fc], 0
1804b8405 mov      rbx, r8
1804b8408 mov      rdi, rcx
1804b840b jne      0x1804b845c
1804b840d lea      rcx, [rip + 0x2cea1c4]
1804b8414 call     0x180309af0 ; 
1804b8419 lea      rcx, [rip + 0x2cbd718]
1804b8420 call     0x180309af0 ; 
1804b8425 lea      rcx, [rip + 0x2cf5ffc]
1804b842c call     0x180309af0 ; 
1804b8431 lea      rcx, [rip + 0x2cf6810]
1804b8438 call     0x180309af0 ; 
1804b843d lea      rcx, [rip + 0x2cf6bc4]
1804b8444 call     0x180309af0 ; 
1804b8449 lea      rcx, [rip + 0x2cf6f78]
1804b8450 call     0x180309af0 ; 
1804b8455 mov      byte ptr [rip + 0x2e851a5], 1
1804b845c mov      rax, qword ptr [rip + 0x2cf5fc5]
1804b8463 cmp      dword ptr [rax + 0xe4], 0
1804b846a jne      0x1804b847b
1804b846c mov      rcx, rax
1804b846f call     0x180309de0 ; 
1804b8474 mov      rax, qword ptr [rip + 0x2cf5fad]
1804b847b mov      rax, qword ptr [rax + 0xb8]
1804b8482 mov      qword ptr [rsp + 0x1b0], rsi
1804b848a mov      qword ptr [rsp + 0x178], r12
1804b8492 mov      qword ptr [rsp + 0x170], r13
1804b849a mov      rcx, qword ptr [rax]
1804b849d mov      qword ptr [rsp + 0x168], r14
1804b84a5 mov      qword ptr [rsp + 0x160], r15
1804b84ad movaps   xmmword ptr [rsp + 0x150], xmm6
1804b84b5 movaps   xmmword ptr [rsp + 0x140], xmm7
1804b84bd movaps   xmmword ptr [rsp + 0x130], xmm8
1804b84c6 movaps   xmmword ptr [rsp + 0x120], xmm9
1804b84cf movaps   xmmword ptr [rsp + 0x110], xmm10
1804b84d8 movaps   xmmword ptr [rsp + 0x100], xmm11
1804b84e1 movaps   xmmword ptr [rsp + 0xf0], xmm12
1804b84ea movaps   xmmword ptr [rsp + 0xe0], xmm13
1804b84f3 test     rcx, rcx
1804b84f6 je       0x1804b9016
1804b84fc xor      edx, edx
1804b84fe call     0x1804ce010 ; 9751:_K._NH._EoA
1804b8503 test     rbx, rbx
1804b8506 je       0x1804b9016
1804b850c test     al, al
1804b850e jne      0x1804b8fca
1804b8514 cmp      byte ptr [rbx + 0x2c], 3
1804b8518 je       0x1804b8fca
1804b851e cmp      byte ptr [rbx + 0x2c], 1
1804b8522 je       0x1804b853e
1804b8524 cmp      byte ptr [rbx + 0x2c], 2
1804b8528 mov      byte ptr [rsp + 0x61], al
1804b852c jne      0x1804b8537
1804b852e movzx    eax, byte ptr [rbx + 0x270]
1804b8535 jmp      0x1804b8550 ; 
1804b8537 mov      byte ptr [rsp + 0x60], 0
1804b853c jmp      0x1804b8554 ; 
1804b853e movzx    eax, byte ptr [rbx + 0x298]
1804b8545 mov      byte ptr [rsp + 0x61], al
1804b8549 movzx    eax, byte ptr [rbx + 0x299]
1804b8550 mov      byte ptr [rsp + 0x60], al
1804b8554 mov      r9d, dword ptr [rbx + 0x1cc]
1804b855b mov      r8, qword ptr [rbx + 0x238]
1804b8562 nop      dword ptr [rax]
1804b8566 nop      word ptr [rax + rax]
1804b8570 xor      r12d, r12d
1804b8573 mov      edi, r12d
1804b8576 test     r8, r8
1804b8579 je       0x1804b9016
1804b857f nop      
1804b8580 cmp      edi, dword ptr [r8 + 0x18]
1804b8584 jae      0x1804b901c
1804b858a movsxd   rax, edi
1804b858d mov      ecx, 0xa
1804b8592 cmp      r9d, dword ptr [r8 + rax*4 + 0x20]
1804b8597 jl       0x1804b85d4
1804b8599 test     r8, r8
1804b859c je       0x1804b9016
1804b85a2 mov      rdx, qword ptr [rbx + 0x240]
1804b85a9 test     rdx, rdx
1804b85ac je       0x1804b9016
1804b85b2 cmp      edi, dword ptr [rdx + 0x18]
1804b85b5 jae      0x1804b901c
1804b85bb movsxd   rax, edi
1804b85be movsxd   rcx, edi
1804b85c1 mov      edx, dword ptr [rdx + rax*4 + 0x20]
1804b85c5 add      edx, dword ptr [r8 + rcx*4 + 0x20]
1804b85ca cmp      r9d, edx
1804b85cd jl       0x1804b8626
1804b85cf mov      ecx, 0xa
1804b85d4 inc      edi
1804b85d6 cmp      edi, 5
1804b85d9 jl       0x1804b8580
1804b85db movzx    edi, cx
1804b85de mov      r8d, dword ptr [rbx + 0x1cc]
1804b85e5 mov      rdx, qword ptr [rbx + 0x238]
1804b85ec nop      dword ptr [rax]
1804b85f0 mov      ecx, r12d
1804b85f3 test     rdx, rdx
1804b85f6 je       0x1804b9016
1804b85fc nop      dword ptr [rax]
1804b8600 cmp      ecx, dword ptr [rdx + 0x18]
1804b8603 jae      0x1804b901c
1804b8609 movsxd   rax, ecx
1804b860c xorps    xmm6, xmm6
1804b860f mov      esi, 4
1804b8614 cmp      dword ptr [rdx + rax*4 + 0x20], r8d
1804b8619 jg       0x1804b862f
1804b861b inc      ecx
1804b861d cmp      ecx, 5
1804b8620 jl       0x1804b8600
1804b8622 mov      ecx, esi
1804b8624 jmp      0x1804b8634 ; 
1804b8626 add      di, di
1804b8629 or       di, 1
1804b862d jmp      0x1804b85de ; 
1804b862f sub      ecx, 1
1804b8632 js       0x1804b8658
1804b8634 mov      rdx, qword ptr [rbx + 0x248]
1804b863b test     rdx, rdx
1804b863e je       0x1804b9016
1804b8644 cmp      ecx, dword ptr [rdx + 0x18]
1804b8647 jae      0x1804b901c
1804b864d mov      eax, ecx
1804b864f movsd    xmm13, qword ptr [rdx + rax*8 + 0x20]
1804b8656 jmp      0x1804b865c ; 
1804b8658 xorps    xmm13, xmm13
1804b865c mov      edx, dword ptr [rbx + 0x1cc]
1804b8662 xor      r8d, r8d
1804b8665 mov      rcx, rbx
1804b8668 call     0x1804b8220 ; 9634:_K._Uh._sMA
1804b866d mov      rdx, qword ptr [rbx + 0x238]
1804b8674 movzx    r15d, al
1804b8678 mov      r14d, dword ptr [rbx + 0x1cc]
1804b867f nop      
1804b8680 mov      ecx, r12d
1804b8683 test     rdx, rdx
1804b8686 je       0x1804b9016
1804b868c nop      dword ptr [rax]
1804b8690 cmp      ecx, dword ptr [rdx + 0x18]
1804b8693 jae      0x1804b901c
1804b8699 movsxd   rax, ecx
1804b869c cmp      dword ptr [rdx + rax*4 + 0x20], r14d
1804b86a1 jg       0x1804b86ac
1804b86a3 inc      ecx
1804b86a5 cmp      ecx, 5
1804b86a8 jl       0x1804b8690
1804b86aa jmp      0x1804b86af ; 
1804b86ac lea      esi, [rcx - 1]
1804b86af mov      rcx, qword ptr [rip + 0x2cf6d12]
1804b86b6 movsxd   rax, esi
1804b86b9 movsx    r13d, word ptr [rbx + rax*2 + 0x218]
1804b86c2 movsxd   rax, esi
1804b86c5 movsx    esi, word ptr [rbx + rax*2 + 0x22c]
1804b86cd cmp      dword ptr [rcx + 0xe4], r12d
1804b86d4 jne      0x1804b86db
1804b86d6 call     0x180309de0 ; 
1804b86db movzx    edx, byte ptr [rbp + 0xc8]
1804b86e2 mov      r12d, 3
1804b86e8 movzx    ecx, byte ptr [rbp + 0xd0]
1804b86ef cmp      dl, 4
1804b86f2 jb       0x1804b86f9
1804b86f4 cmp      cl, 1
1804b86f7 je       0x1804b8703
1804b86f9 test     r15b, r15b
1804b86fc je       0x1804b870b
1804b86fe cmp      dl, 4
1804b8701 jb       0x1804b870b
1804b8703 mov      r15d, r12d
1804b8706 xor      r8d, r8d
1804b8709 jmp      0x1804b8734 ; 
1804b870b lea      eax, [rdx - 2]
1804b870e cmp      eax, 1
1804b8711 ja       0x1804b871e
1804b8713 mov      r15d, 1
1804b8719 xor      r8d, r8d
1804b871c jmp      0x1804b8734 ; 
1804b871e mov      r15d, 2
1804b8724 lea      eax, [rdx - 4]
1804b8727 cmp      eax, r15d
1804b872a mov      r8d, 0
1804b8730 cmova    r15d, r8d
1804b8734 cmp      cl, 4
1804b8737 jb       0x1804b873e
1804b8739 cmp      dl, 1
1804b873c je       0x1804b875e
1804b873e lea      eax, [rcx - 2]
1804b8741 cmp      eax, 1
1804b8744 ja       0x1804b874e
1804b8746 mov      r12d, 1
1804b874c jmp      0x1804b875e ; 
1804b874e mov      r12d, 2
1804b8754 lea      eax, [rcx - 4]
1804b8757 cmp      eax, r12d
1804b875a cmova    r12d, r8d
1804b875e mov      rcx, qword ptr [rbx + 0x2a0]
1804b8765 test     rcx, rcx
1804b8768 je       0x1804b9016
1804b876e xor      r8d, r8d
1804b8771 mov      rdx, rbx
1804b8774 call     0x180494530 ; 9631:ifapp.Game.Data.GameplayHistory._aMA
1804b8779 mov      r8, qword ptr [rbx + 0x10]
1804b877d test     r8, r8
1804b8780 je       0x1804b9016
1804b8786 mov      r9, qword ptr [rbx + 0x18]
1804b878a test     r9, r9
1804b878d je       0x1804b9016
1804b8793 mov      rdx, qword ptr [rbp + 0xb8]
1804b879a mov      rcx, r8
1804b879d mov      rax, qword ptr [rbp + 0xa8]
1804b87a4 mov      r8, qword ptr [r8 + 0x90]
1804b87ab mov      qword ptr [rsp + 0x48], 0
1804b87b4 mov      byte ptr [rsp + 0x40], 1
1804b87b9 movups   xmm0, xmmword ptr [rax]
1804b87bc mov      rax, r9
1804b87bf mov      byte ptr [rsp + 0x38], 1
1804b87c4 mov      r9, qword ptr [r9 + 0x90]
1804b87cb mov      qword ptr [rsp + 0x30], rdx
1804b87d0 movzx    edx, di
1804b87d3 movaps   xmmword ptr [rsp + 0x70], xmm0
1804b87d8 mov      rax, qword ptr [rax + 0x88]
1804b87df mov      qword ptr [rsp + 0x28], rax
1804b87e4 mov      rax, qword ptr [rcx + 0x88]
1804b87eb lea      rcx, [rsp + 0x70]
1804b87f0 mov      qword ptr [rsp + 0x20], rax
1804b87f5 call     0x18049df40 ; 9626:_K._sh._wmA
1804b87fa xor      edx, edx
1804b87fc mov      rcx, rbx
1804b87ff call     0x1804b9030 ; 9636:_K._Vh._XMA
1804b8804 mov      rax, qword ptr [rbp + 0xa8]
1804b880b lea      rcx, [rsp + 0x70]
1804b8810 mov      r9, qword ptr [rbp + 0xc0]
1804b8817 mov      rdx, rbx
1804b881a mov      r8, qword ptr [rbp + 0xb8]
1804b8821 mov      qword ptr [rsp + 0x28], 0
1804b882a movups   xmm0, xmmword ptr [rax]
1804b882d movsd    qword ptr [rsp + 0x20], xmm13
1804b8834 movaps   xmmword ptr [rsp + 0x70], xmm0
1804b8839 call     0x1804ba520 ; 9636:_K._Vh._xMA
1804b883e mov      rax, qword ptr [rbp + 0xb8]
1804b8845 test     rax, rax
1804b8848 je       0x1804b9016
1804b884e mov      rdx, qword ptr [rip + 0x2cbd2e3]
1804b8855 mov      rcx, rax
1804b8858 call     0x180fa0d20 ; 
1804b885d xor      edx, edx
1804b885f mov      rcx, rbx
1804b8862 call     0x1804b9030 ; 9636:_K._Vh._XMA
1804b8867 mov      rcx, qword ptr [rbx + 0x2a0]
1804b886e test     rcx, rcx
1804b8871 je       0x1804b9016
1804b8877 xor      r8d, r8d
1804b887a mov      rdx, rbx
1804b887d call     0x1804939c0 ; 9631:ifapp.Game.Data.GameplayHistory._AMA
1804b8882 cmp      r15d, 3
1804b8886 jne      0x1804b88a8
1804b8888 mov      rax, qword ptr [rbx + 0x10]
1804b888c test     rax, rax
1804b888f je       0x1804b9016
1804b8895 movsd    xmm0, qword ptr [rax + 0x78]
1804b889a comisd   xmm0, xmm6
1804b889e jbe      0x1804b88a8
1804b88a0 movsd    xmm11, qword ptr [rax + 0x78]
1804b88a6 jmp      0x1804b88cb ; 
1804b88a8 mov      rcx, qword ptr [rip + 0x2cf6b19]
1804b88af cmp      dword ptr [rcx + 0xe4], 0
1804b88b6 jne      0x1804b88bd
1804b88b8 call     0x180309de0 ; 
1804b88bd xor      edx, edx
1804b88bf mov      ecx, r15d
1804b88c2 call     0x1804a5670 ; 
1804b88c7 movaps   xmm11, xmm0
1804b88cb cmp      r12d, 3
1804b88cf jne      0x1804b88f1
1804b88d1 mov      rax, qword ptr [rbx + 0x18]
1804b88d5 test     rax, rax
1804b88d8 je       0x1804b9016
1804b88de movsd    xmm1, qword ptr [rax + 0x78]
1804b88e3 comisd   xmm1, xmm6
1804b88e7 jbe      0x1804b88f1
1804b88e9 movsd    xmm12, qword ptr [rax + 0x78]
1804b88ef jmp      0x1804b8914 ; 
1804b88f1 mov      rcx, qword ptr [rip + 0x2cf6ad0]
1804b88f8 cmp      dword ptr [rcx + 0xe4], 0
1804b88ff jne      0x1804b8906
1804b8901 call     0x180309de0 ; 
1804b8906 xor      edx, edx
1804b8908 mov      ecx, r12d
1804b890b call     0x1804a5670 ; 
1804b8910 movaps   xmm12, xmm0
1804b8914 cmp      r15d, 3
1804b8918 jne      0x1804b893d
1804b891a mov      rax, qword ptr [rbx + 0x10]
1804b891e test     rax, rax
1804b8921 je       0x1804b9016
1804b8927 movsd    xmm1, qword ptr [rax + 0x78]
1804b892c comisd   xmm1, xmm6
1804b8930 jbe      0x1804b893d
1804b8932 movsd    xmm9, qword ptr [rax + 0x78]
1804b8938 xor      r13d, r13d
1804b893b jmp      0x1804b8973 ; 
1804b893d mov      edx, r13d
1804b8940 sub      r14d, esi
1804b8943 mov      rcx, qword ptr [rip + 0x2cf6a7e]
1804b894a xor      r13d, r13d
1804b894d cmp      r14d, edx
1804b8950 mov      esi, r13d
1804b8953 setge    sil
1804b8957 cmp      dword ptr [rcx + 0xe4], r13d
1804b895e jne      0x1804b8965
1804b8960 call     0x180309de0 ; 
1804b8965 xor      edx, edx
1804b8967 lea      ecx, [rsi + 2]
1804b896a call     0x1804a5670 ; 
1804b896f movaps   xmm9, xmm0
1804b8973 mov      rcx, qword ptr [rbx + 0x10]
1804b8977 test     rcx, rcx
1804b897a je       0x1804b9016
1804b8980 mov      rax, qword ptr [rbx + 0x18]
1804b8984 test     rax, rax
1804b8987 je       0x1804b9016
1804b898d movsd    xmm8, qword ptr [rcx + 0x28]
1804b8993 mov      rcx, qword ptr [rip + 0x2cf6a2e]
1804b899a movsd    xmm7, qword ptr [rax + 0x30]
1804b899f cmp      dword ptr [rcx + 0xe4], 0
1804b89a6 jne      0x1804b89ad
1804b89a8 call     0x180309de0 ; 
1804b89ad ucomisd  xmm7, xmm6
1804b89b1 movsd    xmm10, qword ptr [rip + 0x214fb9e]
1804b89ba jp       0x1804b89c2
1804b89bc jne      0x1804b89c2
1804b89be movaps   xmm7, xmm10
1804b89c2 mov      rcx, qword ptr [rbx + 0x18]
1804b89c6 divsd    xmm8, xmm7
1804b89cb mulsd    xmm8, xmm13
1804b89d0 mulsd    xmm8, xmm11
1804b89d5 test     rcx, rcx
1804b89d8 je       0x1804b9016
1804b89de mov      rax, qword ptr [rbx + 0x10]
1804b89e2 test     rax, rax
1804b89e5 je       0x1804b9016
1804b89eb movsd    xmm0, qword ptr [rax + 0x30]
1804b89f0 ucomisd  xmm0, xmm6
1804b89f4 jp       0x1804b89fc
1804b89f6 jne      0x1804b89fc
1804b89f8 movaps   xmm0, xmm10
1804b89fc movsd    xmm7, qword ptr [rcx + 0x28]
1804b8a01 mov      rax, qword ptr [rbx + 0x18]
1804b8a05 divsd    xmm7, xmm0
1804b8a09 movsd    xmm0, qword ptr [rax + 0x30]
1804b8a0e ucomisd  xmm0, xmm6
1804b8a12 mov      rax, qword ptr [rbx + 0x10]
1804b8a16 mulsd    xmm7, xmm13
1804b8a1b mulsd    xmm7, xmm12
1804b8a20 jp       0x1804b8a28
1804b8a22 jne      0x1804b8a28
1804b8a24 movaps   xmm0, xmm10
1804b8a28 movsd    xmm12, qword ptr [rax + 0x28]
1804b8a2e xor      edx, edx
1804b8a30 mov      rax, qword ptr [rbx + 0x18]
1804b8a34 mov      ecx, 2
1804b8a39 divsd    xmm12, xmm0
1804b8a3e mulsd    xmm12, xmm13
1804b8a43 mulsd    xmm12, xmm9
1804b8a48 movsd    xmm9, qword ptr [rax + 0x28]
1804b8a4e mov      rax, qword ptr [rbx + 0x10]
1804b8a52 movsd    xmm11, qword ptr [rax + 0x30]
1804b8a58 call     0x1804a5670 ; 
1804b8a5d ucomisd  xmm11, xmm6
1804b8a62 jp       0x1804b8a6a
1804b8a64 jne      0x1804b8a6a
1804b8a66 movaps   xmm11, xmm10
1804b8a6a mov      rcx, qword ptr [rbx + 0x18]
1804b8a6e divsd    xmm9, xmm11
1804b8a73 mulsd    xmm9, xmm13
1804b8a78 mulsd    xmm9, xmm0
1804b8a7d test     rcx, rcx
1804b8a80 je       0x1804b9016
1804b8a86 mov      rax, qword ptr [rbx + 0x10]
1804b8a8a test     rax, rax
1804b8a8d je       0x1804b9016
1804b8a93 movsd    xmm0, qword ptr [rax + 0x30]
1804b8a98 ucomisd  xmm0, xmm6
1804b8a9c jp       0x1804b8aa4
1804b8a9e jne      0x1804b8aa4
1804b8aa0 movaps   xmm0, xmm10
1804b8aa4 ucomisd  xmm7, xmm6
1804b8aa8 mov      qword ptr [rbp - 0x80], r13
1804b8aac mov      qword ptr [rbp - 0x68], r13
1804b8ab0 mov      dword ptr [rbp - 0x60], r15d
1804b8ab4 mov      dword ptr [rbp - 0x5c], r12d
1804b8ab8 movsd    xmm2, qword ptr [rcx + 0x28]
1804b8abd divsd    xmm2, xmm0
1804b8ac1 xorps    xmm0, xmm0
1804b8ac4 mulsd    xmm2, xmm13
1804b8ac9 movdqu   xmmword ptr [rbp - 0x78], xmm0
1804b8ace mulsd    xmm2, qword ptr [rbx + 0x1f8]
1804b8ad6 jp       0x1804b8adf
1804b8ad8 jne      0x1804b8adf
1804b8ada test     r12d, r12d
1804b8add je       0x1804b8ae8
1804b8adf movsd    qword ptr [rbp - 0x78], xmm7
1804b8ae4 mov      byte ptr [rbp - 0x80], 1
1804b8ae8 ucomisd  xmm8, xmm6
1804b8aed jp       0x1804b8af6
1804b8aef jne      0x1804b8af6
1804b8af1 test     r15d, r15d
1804b8af4 je       0x1804b8b00
1804b8af6 movsd    qword ptr [rbp - 0x68], xmm8
1804b8afc mov      byte ptr [rbp - 0x70], 1
1804b8b00 movaps   xmm0, xmm8
1804b8b04 movaps   xmm1, xmm7
1804b8b07 addsd    xmm0, qword ptr [rbx + 0x2a8]
1804b8b0f movsd    qword ptr [rbx + 0x2a8], xmm0
1804b8b17 addsd    xmm1, qword ptr [rbx + 0x2b0]
1804b8b1f movsd    qword ptr [rbx + 0x2b0], xmm1
1804b8b27 movups   xmm0, xmmword ptr [rbx + 0x2b8]
1804b8b2e addpd    xmm0, xmm6
1804b8b32 movups   xmmword ptr [rbx + 0x2b8], xmm0
1804b8b39 cmp      byte ptr [rbx + 0x2c], 1
1804b8b3d je       0x1804b8b70
1804b8b3f cmp      byte ptr [rbx + 0x2c], 2
1804b8b43 jne      0x1804b8b99
1804b8b45 movsd    xmm0, qword ptr [rbx + 0x260]
1804b8b4d subsd    xmm0, xmm8
1804b8b52 movsd    qword ptr [rbx + 0x260], xmm0
1804b8b5a movsd    xmm1, qword ptr [rbx + 0x250]
1804b8b62 subsd    xmm1, xmm7
1804b8b66 movsd    qword ptr [rbx + 0x250], xmm1
1804b8b6e jmp      0x1804b8b99 ; 
1804b8b70 movsd    xmm0, qword ptr [rbx + 0x288]
1804b8b78 subsd    xmm0, xmm8
1804b8b7d movsd    qword ptr [rbx + 0x288], xmm0
1804b8b85 movsd    xmm1, qword ptr [rbx + 0x278]
1804b8b8d subsd    xmm1, xmm7
1804b8b91 movsd    qword ptr [rbx + 0x278], xmm1
1804b8b99 mov      rcx, qword ptr [rbx + 0x2a0]
1804b8ba0 test     rcx, rcx
1804b8ba3 je       0x1804b9016
1804b8ba9 mov      qword ptr [rsp + 0x50], r13
1804b8bae movaps   xmm3, xmm8
1804b8bb2 mov      qword ptr [rsp + 0x48], rbx
1804b8bb7 mov      r8d, r15d
1804b8bba movsd    qword ptr [rsp + 0x40], xmm2
1804b8bc0 movzx    edx, di
1804b8bc3 movsd    qword ptr [rsp + 0x38], xmm9
1804b8bca movsd    qword ptr [rsp + 0x30], xmm12
1804b8bd1 movsd    qword ptr [rsp + 0x28], xmm7
1804b8bd7 mov      dword ptr [rsp + 0x20], r12d
1804b8bdc call     0x180494720 ; 9631:ifapp.Game.Data.GameplayHistory._bMA
1804b8be1 cmp      byte ptr [rbx + 0x2c], 1
1804b8be5 je       0x1804b8c21
1804b8be7 cmp      byte ptr [rbx + 0x2c], 2
1804b8beb jne      0x1804b8c43
1804b8bed movsd    xmm1, qword ptr [rbx + 0x268]
1804b8bf5 movsd    xmm0, qword ptr [rbx + 0x258]
1804b8bfd subsd    xmm1, qword ptr [rbx + 0x260]
1804b8c05 subsd    xmm0, qword ptr [rbx + 0x250]
1804b8c0d subsd    xmm1, xmm0
1804b8c11 comisd   xmm1, xmm10
1804b8c16 setae    al
1804b8c19 mov      byte ptr [rbx + 0x270], al
1804b8c1f jmp      0x1804b8c43 ; 
1804b8c21 comisd   xmm6, xmmword ptr [rbx + 0x278]
1804b8c29 setae    al
1804b8c2c or       byte ptr [rbx + 0x298], al
1804b8c32 comisd   xmm6, xmmword ptr [rbx + 0x288]
1804b8c3a setae    al
1804b8c3d or       byte ptr [rbx + 0x299], al
1804b8c43 cmp      byte ptr [rsp + 0x61], 0
1804b8c48 jne      0x1804b8cb6
1804b8c4a cmp      byte ptr [rbx + 0x298], 0
1804b8c51 je       0x1804b8cb6
1804b8c53 mov      rax, qword ptr [rip + 0x2cf5fee]
1804b8c5a cmp      dword ptr [rax + 0xe4], 0
1804b8c61 jne      0x1804b8c72
1804b8c63 mov      rcx, rax
1804b8c66 call     0x180309de0 ; 
1804b8c6b mov      rax, qword ptr [rip + 0x2cf5fd6]
1804b8c72 mov      rsi, qword ptr [rbp + 0xc0]
1804b8c79 test     rsi, rsi
1804b8c7c je       0x1804b9016
1804b8c82 mov      rax, qword ptr [rax + 0xb8]
1804b8c89 lea      rdx, [rbp - 0x50]
1804b8c8d mov      r8, qword ptr [rip + 0x2ce9944]
1804b8c94 mov      rcx, rsi
1804b8c97 movups   xmm0, xmmword ptr [rax + 0x30]
1804b8c9b movups   xmm1, xmmword ptr [rax + 0x40]
1804b8c9f movaps   xmmword ptr [rbp - 0x50], xmm0
1804b8ca3 movups   xmm0, xmmword ptr [rax + 0x50]
1804b8ca7 movaps   xmmword ptr [rbp - 0x40], xmm1
1804b8cab movaps   xmmword ptr [rbp - 0x30], xmm0
1804b8caf call     0x180d08aa0 ; 
1804b8cb4 jmp      0x1804b8cbd ; 
1804b8cb6 mov      rsi, qword ptr [rbp + 0xc0]
1804b8cbd cmp      byte ptr [rsp + 0x60], 0
1804b8cc2 jne      0x1804b8d2a
1804b8cc4 cmp      byte ptr [rbx + 0x299], 0
1804b8ccb je       0x1804b8d2a
1804b8ccd mov      rax, qword ptr [rip + 0x2cf5f74]
1804b8cd4 cmp      dword ptr [rax + 0xe4], 0
1804b8cdb jne      0x1804b8cec
1804b8cdd mov      rcx, rax
1804b8ce0 call     0x180309de0 ; 
1804b8ce5 mov      rax, qword ptr [rip + 0x2cf5f5c]
1804b8cec test     rsi, rsi
1804b8cef je       0x1804b9016
1804b8cf5 mov      rax, qword ptr [rax + 0xb8]
1804b8cfc lea      rdx, [rbp - 0x50]
1804b8d00 mov      r8, qword ptr [rip + 0x2ce98d1]
1804b8d07 mov      rcx, rsi
1804b8d0a movups   xmm0, xmmword ptr [rax + 0x60]
1804b8d0e movups   xmm1, xmmword ptr [rax + 0x70]
1804b8d12 movaps   xmmword ptr [rbp - 0x50], xmm0
1804b8d16 movups   xmm0, xmmword ptr [rax + 0x80]
1804b8d1d movaps   xmmword ptr [rbp - 0x40], xmm1
1804b8d21 movaps   xmmword ptr [rbp - 0x30], xmm0
1804b8d25 call     0x180d08aa0 ; 
1804b8d2a movzx    edx, byte ptr [rbp + 0xc8]
1804b8d31 xor      r9d, r9d
1804b8d34 inc      dword ptr [rbx + 0x1cc]
1804b8d3a movzx    r8d, di
1804b8d3e mov      rcx, rbx
1804b8d41 call     0x1804b7ef0 ; 9634:_K._Uh._pMA
1804b8d46 mov      eax, dword ptr [rbx + 0x1cc]
1804b8d4c mov      dword ptr [rbx + 0x2d8], eax
1804b8d52 mov      eax, dword ptr [rbx + 0x1c8]
1804b8d58 mov      dword ptr [rbx + 0x2dc], eax
1804b8d5e mov      r9d, dword ptr [rbx + 0x1cc]
1804b8d65 test     r9d, r9d
1804b8d68 je       0x1804b8e3d
1804b8d6e mov      r8, qword ptr [rbx + 0x238]
1804b8d75 nop      word ptr [rax + rax]
1804b8d80 mov      r10d, r13d
1804b8d83 test     r8, r8
1804b8d86 je       0x1804b9016
1804b8d8c nop      dword ptr [rax]
1804b8d90 cmp      r10d, dword ptr [r8 + 0x18]
1804b8d94 jae      0x1804b901c
1804b8d9a movsxd   rax, r10d
1804b8d9d cmp      r9d, dword ptr [r8 + rax*4 + 0x20]
1804b8da2 jle      0x1804b8de4
1804b8da4 test     r8, r8
1804b8da7 je       0x1804b9016
1804b8dad mov      rdx, qword ptr [rbx + 0x240]
1804b8db4 test     rdx, rdx
1804b8db7 je       0x1804b9016
1804b8dbd cmp      r10d, dword ptr [rdx + 0x18]
1804b8dc1 jae      0x1804b901c
1804b8dc7 movsxd   rax, r10d
1804b8dca movsxd   rcx, r10d
1804b8dcd mov      edx, dword ptr [rdx + rax*4 + 0x20]
1804b8dd1 add      edx, dword ptr [r8 + rcx*4 + 0x20]
1804b8dd6 cmp      r9d, edx
1804b8dd9 jl       0x1804b8e27
1804b8ddb mov      rdx, qword ptr [rbx + 0x240]
1804b8de2 jmp      0x1804b8dfe ; 
1804b8de4 mov      rdx, qword ptr [rbx + 0x240]
1804b8deb test     rdx, rdx
1804b8dee je       0x1804b9016
1804b8df4 cmp      r10d, dword ptr [rdx + 0x18]
1804b8df8 jae      0x1804b901c
1804b8dfe movsxd   rax, r10d
1804b8e01 movsxd   rcx, r10d
1804b8e04 mov      edx, dword ptr [rdx + rax*4 + 0x20]
1804b8e08 add      edx, dword ptr [r8 + rcx*4 + 0x20]
1804b8e0d cmp      r9d, edx
1804b8e10 je       0x1804b8e32
1804b8e12 inc      r10d
1804b8e15 cmp      r10d, 5
1804b8e19 jl       0x1804b8d90
1804b8e1f mov      r10d, 0xa
1804b8e25 jmp      0x1804b8e41 ; 
1804b8e27 add      r10w, r10w
1804b8e2b or       r10w, 1
1804b8e30 jmp      0x1804b8e41 ; 
1804b8e32 add      r10w, r10w
1804b8e36 add      r10w, 2
1804b8e3b jmp      0x1804b8e41 ; 
1804b8e3d movzx    r10d, r13w
1804b8e41 mov      r8, qword ptr [rbx + 0x10]
1804b8e45 test     r8, r8
1804b8e48 je       0x1804b9016
1804b8e4e mov      r9, qword ptr [rbx + 0x18]
1804b8e52 test     r9, r9
1804b8e55 je       0x1804b9016
1804b8e5b mov      r14, qword ptr [rbp + 0xa8]
1804b8e62 mov      rcx, r8
1804b8e65 mov      rdi, qword ptr [rbp + 0xb8]
1804b8e6c mov      rax, r9
1804b8e6f mov      r9, qword ptr [r9 + 0x90]
1804b8e76 movzx    edx, r10w
1804b8e7a mov      r8, qword ptr [r8 + 0x90]
1804b8e81 movups   xmm0, xmmword ptr [r14]
1804b8e85 mov      rax, qword ptr [rax + 0x88]
1804b8e8c mov      qword ptr [rsp + 0x48], r13
1804b8e91 mov      byte ptr [rsp + 0x40], 1
1804b8e96 mov      byte ptr [rsp + 0x38], 0
1804b8e9b mov      qword ptr [rsp + 0x30], rdi
1804b8ea0 mov      qword ptr [rsp + 0x28], rax
1804b8ea5 mov      rax, qword ptr [rcx + 0x88]
1804b8eac lea      rcx, [rsp + 0x70]
1804b8eb1 mov      qword ptr [rsp + 0x20], rax
1804b8eb6 movaps   xmmword ptr [rsp + 0x70], xmm0
1804b8ebb call     0x18049df40 ; 9626:_K._sh._wmA
1804b8ec0 mov      rcx, qword ptr [rip + 0x2cf6501]
1804b8ec7 cmp      dword ptr [rcx + 0xe4], 0
1804b8ece jne      0x1804b8ed5
1804b8ed0 call     0x180309de0 ; 
1804b8ed5 xor      edx, edx
1804b8ed7 mov      rcx, rbx
1804b8eda call     0x1804b9030 ; 9636:_K._Vh._XMA
1804b8edf movups   xmm0, xmmword ptr [r14]
1804b8ee3 mov      qword ptr [rsp + 0x28], r13
1804b8ee8 lea      rcx, [rsp + 0x70]
1804b8eed mov      r9, rsi
1804b8ef0 movsd    qword ptr [rsp + 0x20], xmm13
1804b8ef7 mov      r8, rdi
1804b8efa movaps   xmmword ptr [rsp + 0x70], xmm0
1804b8eff mov      rdx, rbx
1804b8f02 call     0x1804ba520 ; 9636:_K._Vh._xMA
1804b8f07 mov      rdx, qword ptr [rip + 0x2cbcc2a]
1804b8f0e mov      rcx, rdi
1804b8f11 call     0x180fa0d20 ; 
1804b8f16 mov      rcx, qword ptr [rbx + 0x2a0]
1804b8f1d test     rcx, rcx
1804b8f20 je       0x1804b9016
1804b8f26 xor      r8d, r8d
1804b8f29 mov      rdx, rbx
1804b8f2c call     0x180494440 ; 9631:ifapp.Game.Data.GameplayHistory._ZmA
1804b8f31 mov      rax, qword ptr [rbp + 0xa0]
1804b8f38 movups   xmm0, xmmword ptr [rbp - 0x80]
1804b8f3c movups   xmm1, xmmword ptr [rbp - 0x70]
1804b8f40 movups   xmmword ptr [rax], xmm0
1804b8f43 movsd    xmm0, qword ptr [rbp - 0x60]
1804b8f48 movups   xmmword ptr [rax + 0x10], xmm1
1804b8f4c movsd    qword ptr [rax + 0x20], xmm0
1804b8f51 movaps   xmm13, xmmword ptr [rsp + 0xe0]
1804b8f5a movaps   xmm12, xmmword ptr [rsp + 0xf0]
1804b8f63 movaps   xmm11, xmmword ptr [rsp + 0x100]
1804b8f6c movaps   xmm10, xmmword ptr [rsp + 0x110]
1804b8f75 movaps   xmm9, xmmword ptr [rsp + 0x120]
1804b8f7e movaps   xmm8, xmmword ptr [rsp + 0x130]
1804b8f87 movaps   xmm7, xmmword ptr [rsp + 0x140]
1804b8f8f movaps   xmm6, xmmword ptr [rsp + 0x150]
1804b8f97 mov      r15, qword ptr [rsp + 0x160]
1804b8f9f mov      r14, qword ptr [rsp + 0x168]
1804b8fa7 mov      r13, qword ptr [rsp + 0x170]
1804b8faf mov      r12, qword ptr [rsp + 0x178]
1804b8fb7 mov      rsi, qword ptr [rsp + 0x1b0]
1804b8fbf add      rsp, 0x180
1804b8fc6 pop      rdi
1804b8fc7 pop      rbx
1804b8fc8 pop      rbp
1804b8fc9 ret      
1804b8fca inc      dword ptr [rbx + 0x1cc]
1804b8fd0 xor      eax, eax
1804b8fd2 movzx    edx, byte ptr [rbp + 0xc8]
1804b8fd9 movzx    r8d, ax
1804b8fdd xor      r9d, r9d
1804b8fe0 mov      rcx, rbx
1804b8fe3 call     0x1804b7ef0 ; 9634:_K._Uh._pMA
1804b8fe8 mov      rax, qword ptr [rip + 0x2cf6019]
1804b8fef mov      rcx, qword ptr [rax + 0xb8]
1804b8ff6 mov      rax, rdi
1804b8ff9 movups   xmm0, xmmword ptr [rcx]
1804b8ffc movups   xmm1, xmmword ptr [rcx + 0x10]
1804b9000 movups   xmmword ptr [rdi], xmm0
1804b9003 movsd    xmm0, qword ptr [rcx + 0x20]
1804b9008 movups   xmmword ptr [rdi + 0x10], xmm1
1804b900c movsd    qword ptr [rdi + 0x20], xmm0
1804b9011 jmp      0x1804b8f51 ; 
1804b9016 call     0x180309d40 ; 
1804b901c call     0x180309d30 ; 

FUNCTION 9636 _Vh _xMA 0x1804ba520 3920
1804ba520 mov      qword ptr [rsp + 0x20], r9
1804ba525 mov      qword ptr [rsp + 0x18], r8
1804ba52a mov      qword ptr [rsp + 8], rcx
1804ba52f push     rbp
1804ba530 push     rsi
1804ba531 push     r12
1804ba533 push     r15
1804ba535 lea      rbp, [rsp - 0x88]
1804ba53d sub      rsp, 0x188
1804ba544 cmp      byte ptr [rip + 0x2e830b7], 0
1804ba54b mov      r12, r9
1804ba54e mov      r15, r8
1804ba551 mov      rsi, rdx
1804ba554 jne      0x1804ba5fd
1804ba55a lea      rcx, [rip + 0x2ce7577]
1804ba561 call     0x180309af0 ; 
1804ba566 lea      rcx, [rip + 0x2d0c153]
1804ba56d call     0x180309af0 ; 
1804ba572 lea      rcx, [rip + 0x2ce805f]
1804ba579 call     0x180309af0 ; 
1804ba57e lea      rcx, [rip + 0x2cbb943]
1804ba585 call     0x180309af0 ; 
1804ba58a lea      rcx, [rip + 0x2cbb63f]
1804ba591 call     0x180309af0 ; 
1804ba596 lea      rcx, [rip + 0x2cbb9c3]
1804ba59d call     0x180309af0 ; 
1804ba5a2 lea      rcx, [rip + 0x2cbb6bf]
1804ba5a9 call     0x180309af0 ; 
1804ba5ae lea      rcx, [rip + 0x2cbba43]
1804ba5b5 call     0x180309af0 ; 
1804ba5ba lea      rcx, [rip + 0x2cbb73f]
1804ba5c1 call     0x180309af0 ; 
1804ba5c6 lea      rcx, [rip + 0x2cbbac3]
1804ba5cd call     0x180309af0 ; 
1804ba5d2 lea      rcx, [rip + 0x2cbb7bf]
1804ba5d9 call     0x180309af0 ; 
1804ba5de lea      rcx, [rip + 0x2cf4203]
1804ba5e5 call     0x180309af0 ; 
1804ba5ea lea      rcx, [rip + 0x2cf4dd7]
1804ba5f1 call     0x180309af0 ; 
1804ba5f6 mov      byte ptr [rip + 0x2e83005], 1
1804ba5fd mov      qword ptr [rsp + 0x1b8], rbx
1804ba605 xorps    xmm0, xmm0
1804ba608 mov      qword ptr [rsp + 0x180], rdi
1804ba610 mov      qword ptr [rsp + 0x178], r13
1804ba618 mov      qword ptr [rsp + 0x170], r14
1804ba620 movaps   xmmword ptr [rsp + 0x160], xmm6
1804ba628 movaps   xmmword ptr [rsp + 0x150], xmm7
1804ba630 movaps   xmmword ptr [rsp + 0x140], xmm8
1804ba639 movaps   xmmword ptr [rsp + 0x130], xmm9
1804ba642 movaps   xmmword ptr [rsp + 0x120], xmm10
1804ba64b movaps   xmmword ptr [rsp + 0x110], xmm11
1804ba654 movaps   xmmword ptr [rsp + 0x100], xmm12
1804ba65d movaps   xmmword ptr [rsp + 0xf0], xmm13
1804ba666 movaps   xmmword ptr [rsp + 0xe0], xmm14
1804ba66f movups   xmmword ptr [rsp + 0x48], xmm0
1804ba674 movups   xmmword ptr [rsp + 0x58], xmm0
1804ba679 movups   xmmword ptr [rsp + 0x68], xmm0
1804ba67e test     r15, r15
1804ba681 je       0x1804bb455
1804ba687 movsd    xmm12, qword ptr [rip + 0x214df10]
1804ba690 xor      r13d, r13d
1804ba693 movsd    xmm14, qword ptr [rip + 0x214daa4]
1804ba69c xorps    xmm11, xmm11
1804ba6a0 movsd    xmm13, qword ptr [rip + 0x214deaf]
1804ba6a9 mov      dword ptr [rsp + 0x44], r13d
1804ba6ae nop      
1804ba6b0 mov      r9, qword ptr [rip + 0x2cf4131]
1804ba6b7 mov      rdi, qword ptr [r15 + 0x10]
1804ba6bb mov      ebx, dword ptr [r15 + 0x18]
1804ba6bf cmp      dword ptr [r9 + 0xe4], 0
1804ba6c7 jne      0x1804ba6d8
1804ba6c9 mov      rcx, r9
1804ba6cc call     0x180309de0 ; 
1804ba6d1 mov      r9, qword ptr [rip + 0x2cf4110]
1804ba6d8 mov      r9, qword ptr [r9 + 0xb8]
1804ba6df sub      ebx, r13d
1804ba6e2 mov      rax, qword ptr [rip + 0x2ce73ef]
1804ba6e9 mov      r8d, ebx
1804ba6ec mov      edx, r13d
1804ba6ef mov      qword ptr [rsp + 0x20], rax
1804ba6f4 mov      rcx, rdi
1804ba6f7 mov      r9, qword ptr [r9]
1804ba6fa call     0x1807e3280 ; 
1804ba6ff xor      r14b, r14b
1804ba702 mov      byte ptr [rsp + 0x40], r14b
1804ba707 nop      word ptr [rax + rax]
1804ba710 cmp      r13d, dword ptr [r15 + 0x18]
1804ba714 jge      0x1804bb36f
1804ba71a mov      r8, qword ptr [rip + 0x2cbb547]
1804ba721 mov      edx, r13d
1804ba724 mov      rcx, r15
1804ba727 call     0x180fac2f0 ; 
1804ba72c mov      r15, rax
1804ba72f mov      eax, dword ptr [rax + 4]
1804ba732 sub      eax, 1
1804ba735 je       0x1804bb21b
1804ba73b sub      eax, 1
1804ba73e je       0x1804bb11f
1804ba744 sub      eax, 1
1804ba747 je       0x1804ba9e6
1804ba74d sub      eax, 1
1804ba750 je       0x1804ba8c1
1804ba756 cmp      eax, 1
1804ba759 je       0x1804ba76c
1804ba75b inc      r13d
1804ba75e mov      dword ptr [rsp + 0x44], r13d
1804ba763 mov      r15, qword ptr [rbp + 0xc0]
1804ba76a jmp      0x1804ba710 ; 
1804ba76c cmp      dword ptr [r15 + 0x14], 0
1804ba771 jne      0x1804ba782
1804ba773 test     rsi, rsi
1804ba776 je       0x1804bb455
1804ba77c mov      r14, qword ptr [rsi + 0x10]
1804ba780 jmp      0x1804ba78f ; 
1804ba782 test     rsi, rsi
1804ba785 je       0x1804bb455
1804ba78b mov      r14, qword ptr [rsi + 0x18]
1804ba78f test     r14, r14
1804ba792 je       0x1804bb455
1804ba798 mov      r14, qword ptr [r14 + 0x90]
1804ba79f nop      
1804ba7a0 xor      r12d, r12d
1804ba7a3 test     r14, r14
1804ba7a6 je       0x1804bb455
1804ba7ac nop      dword ptr [rax]
1804ba7b0 cmp      r12d, dword ptr [r14 + 0x18]
1804ba7b4 jae      0x1804bb45b
1804ba7ba movsxd   rdi, r12d
1804ba7bd add      rdi, rdi
1804ba7c0 xor      ebx, ebx
1804ba7c2 mov      rax, qword ptr [r14 + rdi*8 + 0x20]
1804ba7c7 test     rax, rax
1804ba7ca je       0x1804bb455
1804ba7d0 cmp      ebx, dword ptr [rax + 0x18]
1804ba7d3 jge      0x1804ba81b
1804ba7d5 mov      r8, qword ptr [rip + 0x2cbb81c]
1804ba7dc mov      edx, ebx
1804ba7de mov      rcx, rax
1804ba7e1 call     0x180fac330 ; 
1804ba7e6 mov      rcx, rax
1804ba7e9 lea      rdx, [r15 + 0xc]
1804ba7ed xor      r8d, r8d
1804ba7f0 call     0x1804d8500 ; 9786:_K._vH._dpA
1804ba7f5 test     al, al
1804ba7f7 je       0x1804ba817
1804ba7f9 mov      rcx, qword ptr [r14 + rdi*8 + 0x20]
1804ba7fe test     rcx, rcx
1804ba801 je       0x1804bb455
1804ba807 mov      r8, qword ptr [rip + 0x2cbb752]
1804ba80e mov      edx, ebx
1804ba810 dec      ebx
1804ba812 call     0x180fab1f0 ; 
1804ba817 inc      ebx
1804ba819 jmp      0x1804ba7c2 ; 
1804ba81b inc      r12d
1804ba81e cmp      r12d, 5
1804ba822 jl       0x1804ba7b0
1804ba824 movups   xmm1, xmmword ptr [r15 + 0x24]
1804ba829 mov      r12, qword ptr [rbp + 0xc8]
1804ba830 movzx    eax, byte ptr [r15 + 0x14]
1804ba835 xorps    xmm0, xmm0
1804ba838 movups   xmmword ptr [rsp + 0x48], xmm0
1804ba83d mov      byte ptr [rsp + 0x48], 0x15
1804ba842 mov      byte ptr [rsp + 0x54], al
1804ba846 movups   xmmword ptr [rsp + 0x58], xmm0
1804ba84b movups   xmmword ptr [rsp + 0x68], xmm0
1804ba850 movsd    xmm0, qword ptr [r15 + 0xc]
1804ba856 movsd    qword ptr [rsp + 0x4c], xmm0
1804ba85c movups   xmm0, xmmword ptr [r15 + 0x15]
1804ba861 movups   xmmword ptr [rsp + 0x55], xmm0
1804ba866 movups   xmmword ptr [rsp + 0x64], xmm1
1804ba86b test     r12, r12
1804ba86e je       0x1804bb455
1804ba874 movups   xmm0, xmmword ptr [rsp + 0x48]
1804ba879 mov      r8, qword ptr [rip + 0x2ce7d58]
1804ba880 lea      rdx, [rbp - 0x80]
1804ba884 movups   xmm1, xmmword ptr [rsp + 0x58]
1804ba889 mov      rcx, r12
1804ba88c movaps   xmmword ptr [rbp - 0x80], xmm0
1804ba890 movups   xmm0, xmmword ptr [rsp + 0x68]
1804ba895 movaps   xmmword ptr [rbp - 0x70], xmm1
1804ba899 movaps   xmmword ptr [rbp - 0x60], xmm0
1804ba89d call     0x180d08aa0 ; 
1804ba8a2 mov      r13d, dword ptr [rsp + 0x44]
1804ba8a7 movzx    r14d, byte ptr [rsp + 0x40]
1804ba8ad inc      r13d
1804ba8b0 mov      dword ptr [rsp + 0x44], r13d
1804ba8b5 mov      r15, qword ptr [rbp + 0xc0]
1804ba8bc jmp      0x1804ba710 ; 
1804ba8c1 cmp      dword ptr [r15 + 0x14], 0
1804ba8c6 jne      0x1804ba8d7
1804ba8c8 test     rsi, rsi
1804ba8cb je       0x1804bb455
1804ba8d1 mov      rdi, qword ptr [rsi + 0x10]
1804ba8d5 jmp      0x1804ba8e4 ; 
1804ba8d7 test     rsi, rsi
1804ba8da je       0x1804bb455
1804ba8e0 mov      rdi, qword ptr [rsi + 0x18]
1804ba8e4 test     rdi, rdi
1804ba8e7 je       0x1804bb455
1804ba8ed mov      rdi, qword ptr [rdi + 0x90]
1804ba8f4 mov      ebx, dword ptr [r15 + 0x24]
1804ba8f8 cmp      ebx, dword ptr [r15 + 0x28]
1804ba8fc jg       0x1804ba95b
1804ba8fe nop      
1804ba900 test     rdi, rdi
1804ba903 je       0x1804bb455
1804ba909 cmp      ebx, dword ptr [rdi + 0x18]
1804ba90c jae      0x1804bb45b
1804ba912 movsxd   rax, ebx
1804ba915 add      rax, rax
1804ba918 mov      rcx, qword ptr [rdi + rax*8 + 0x20]
1804ba91d test     rcx, rcx
1804ba920 je       0x1804bb455
1804ba926 movups   xmm0, xmmword ptr [r15 + 0xc]
1804ba92b mov      r8, qword ptr [rip + 0x2cbb596]
1804ba932 lea      rdx, [rbp - 0x80]
1804ba936 movups   xmm1, xmmword ptr [r15 + 0x1c]
1804ba93b movaps   xmmword ptr [rbp - 0x80], xmm0
1804ba93f movsd    xmm0, qword ptr [r15 + 0x2c]
1804ba945 movsd    qword ptr [rbp - 0x60], xmm0
1804ba94a movaps   xmmword ptr [rbp - 0x70], xmm1
1804ba94e call     0x180fa5db0 ; 
1804ba953 inc      ebx
1804ba955 cmp      ebx, dword ptr [r15 + 0x28]
1804ba959 jle      0x1804ba900
1804ba95b movzx    eax, byte ptr [r15 + 0x14]
1804ba960 xorps    xmm0, xmm0
1804ba963 movups   xmm1, xmmword ptr [r15 + 0x24]
1804ba968 movups   xmmword ptr [rsp + 0x48], xmm0
1804ba96d mov      byte ptr [rsp + 0x48], 0x14
1804ba972 mov      byte ptr [rsp + 0x54], al
1804ba976 movups   xmmword ptr [rsp + 0x58], xmm0
1804ba97b movups   xmmword ptr [rsp + 0x68], xmm0
1804ba980 movsd    xmm0, qword ptr [r15 + 0xc]
1804ba986 movsd    qword ptr [rsp + 0x4c], xmm0
1804ba98c movups   xmm0, xmmword ptr [r15 + 0x15]
1804ba991 movups   xmmword ptr [rsp + 0x55], xmm0
1804ba996 movups   xmmword ptr [rsp + 0x64], xmm1
1804ba99b test     r12, r12
1804ba99e je       0x1804bb455
1804ba9a4 movups   xmm0, xmmword ptr [rsp + 0x48]
1804ba9a9 mov      r8, qword ptr [rip + 0x2ce7c28]
1804ba9b0 lea      rdx, [rbp - 0x80]
1804ba9b4 movups   xmm1, xmmword ptr [rsp + 0x58]
1804ba9b9 mov      rcx, r12
1804ba9bc movaps   xmmword ptr [rbp - 0x80], xmm0
1804ba9c0 movups   xmm0, xmmword ptr [rsp + 0x68]
1804ba9c5 movaps   xmmword ptr [rbp - 0x70], xmm1
1804ba9c9 movaps   xmmword ptr [rbp - 0x60], xmm0
1804ba9cd call     0x180d08aa0 ; 
1804ba9d2 inc      r13d
1804ba9d5 mov      dword ptr [rsp + 0x44], r13d
1804ba9da mov      r15, qword ptr [rbp + 0xc0]
1804ba9e1 jmp      0x1804ba710 ; 
1804ba9e6 movzx    eax, word ptr [r15 + 0xc]
1804ba9eb mov      ecx, 0x1fa0
1804ba9f0 cmp      ax, cx
1804ba9f3 jne      0x1804bad32
1804ba9f9 cmp      dword ptr [r15 + 8], 0
1804ba9fe je       0x1804bab65
1804baa04 cmp      dword ptr [r15 + 8], 1
1804baa09 jne      0x1804bab57
1804baa0f mov      rcx, qword ptr [rip + 0x2cf49b2]
1804baa16 cmp      dword ptr [rcx + 0xe4], 0
1804baa1d jne      0x1804baa24
1804baa1f call     0x180309de0 ; 
1804baa24 xor      edx, edx
1804baa26 mov      rcx, rsi
1804baa29 call     0x1804b9030 ; 9636:_K._Vh._XMA
1804baa2e test     rsi, rsi
1804baa31 je       0x1804bb455
1804baa37 mov      rcx, qword ptr [rsi + 0x18]
1804baa3b test     rcx, rcx
1804baa3e je       0x1804bb455
1804baa44 mov      rax, qword ptr [rsi + 0x10]
1804baa48 test     rax, rax
1804baa4b je       0x1804bb455
1804baa51 movsd    xmm6, qword ptr [rcx + 0x28]
1804baa56 movsd    xmm7, qword ptr [rax + 0x30]
1804baa5b movsd    xmm8, qword ptr [rax + 0x40]
1804baa61 xor      edx, edx
1804baa63 mov      ecx, 2
1804baa68 call     0x1804a5670 ; 
1804baa6d xorps    xmm9, xmm9
1804baa71 cvtsi2sd xmm9, qword ptr [r15 + 0x24]
1804baa77 divsd    xmm7, xmm8
1804baa7c mulsd    xmm9, xmm12
1804baa81 mulsd    xmm9, xmm6
1804baa86 divsd    xmm9, xmm7
1804baa8b divsd    xmm9, xmm8
1804baa90 cmp      byte ptr [rsi + 0x2c], 1
1804baa94 mov      eax, 0x278
1804baa99 mov      ecx, 0x250
1804baa9e movaps   xmm1, xmm9
1804baaa2 cmovne   eax, ecx
1804baaa5 xorps    xmm2, xmm2
1804baaa8 movups   xmmword ptr [rsp + 0x48], xmm2
1804baaad mov      byte ptr [rsp + 0x48], 9
1804baab2 mov      byte ptr [rsp + 0x54], 5
1804baab7 movsd    xmm0, qword ptr [rsi + rax]
1804baabc subsd    xmm0, xmm9
1804baac1 movsd    qword ptr [rsi + rax], xmm0
1804baac6 movaps   xmm0, xmm9
1804baaca mulsd    xmm0, xmm8
1804baacf addsd    xmm1, qword ptr [rsi + 0x2b0]
1804baad7 mulsd    xmm0, xmm14
1804baadc movsd    qword ptr [rsi + 0x2b0], xmm1
1804baae4 movsd    qword ptr [rsp + 0x4c], xmm0
1804baaea test     r12, r12
1804baaed je       0x1804bb455
1804baaf3 movups   xmm0, xmmword ptr [rsp + 0x48]
1804baaf8 mov      r8, qword ptr [rip + 0x2ce7ad9]
1804baaff lea      rdx, [rbp - 0x50]
1804bab03 mov      rcx, r12
1804bab06 movaps   xmmword ptr [rbp - 0x40], xmm2
1804bab0a movaps   xmmword ptr [rbp - 0x50], xmm0
1804bab0e movaps   xmmword ptr [rbp - 0x30], xmm2
1804bab12 call     0x180d08aa0 ; 
1804bab17 mov      rcx, qword ptr [rsi + 0x2a0]
1804bab1e test     rcx, rcx
1804bab21 je       0x1804bb455
1804bab27 movups   xmm0, xmmword ptr [r15 + 0xc]
1804bab2c lea      r9, [rbp - 0x80]
1804bab30 mov      r8, rsi
1804bab33 movups   xmm2, xmmword ptr [r15 + 0x1c]
1804bab38 mov      qword ptr [rsp + 0x20], 0
1804bab41 movaps   xmm1, xmm9
1804bab45 movaps   xmmword ptr [rbp - 0x80], xmm0
1804bab49 movaps   xmmword ptr [rbp - 0x70], xmm2
1804bab4d call     0x1804950e0 ; 9631:ifapp.Game.Data.GameplayHistory._dMA
1804bab52 jmp      0x1804bacc2 ; 
1804bab57 test     rsi, rsi
1804bab5a je       0x1804bb455
1804bab60 jmp      0x1804bacc2 ; 
1804bab65 mov      rcx, qword ptr [rip + 0x2cf485c]
1804bab6c cmp      dword ptr [rcx + 0xe4], 0
1804bab73 jne      0x1804bab7a
1804bab75 call     0x180309de0 ; 
1804bab7a xor      edx, edx
1804bab7c mov      rcx, rsi
1804bab7f call     0x1804b9030 ; 9636:_K._Vh._XMA
1804bab84 test     rsi, rsi
1804bab87 je       0x1804bb455
1804bab8d mov      rcx, qword ptr [rsi + 0x10]
1804bab91 test     rcx, rcx
1804bab94 je       0x1804bb455
1804bab9a mov      rax, qword ptr [rsi + 0x18]
1804bab9e test     rax, rax
1804baba1 je       0x1804bb455
1804baba7 movsd    xmm6, qword ptr [rcx + 0x28]
1804babac movsd    xmm7, qword ptr [rax + 0x30]
1804babb1 movsd    xmm8, qword ptr [rax + 0x40]
1804babb7 mov      edx, dword ptr [rsi + 0x1cc]
1804babbd xor      r8d, r8d
1804babc0 mov      rcx, rsi
1804babc3 call     0x1804b8220 ; 9634:_K._Uh._sMA
1804babc8 movzx    ecx, al
1804babcb or       ecx, 2
1804babce xor      edx, edx
1804babd0 call     0x1804a5670 ; 
1804babd5 xorps    xmm9, xmm9
1804babd9 cvtsi2sd xmm9, qword ptr [r15 + 0x24]
1804babdf divsd    xmm7, xmm8
1804babe4 mulsd    xmm9, xmm12
1804babe9 mulsd    xmm9, xmm6
1804babee divsd    xmm9, xmm7
1804babf3 divsd    xmm9, xmm8
1804babf8 cmp      byte ptr [rsi + 0x2c], 1
1804babfc mov      eax, 0x288
1804bac01 mov      ecx, 0x260
1804bac06 movaps   xmm1, xmm9
1804bac0a cmovne   eax, ecx
1804bac0d xorps    xmm2, xmm2
1804bac10 movups   xmmword ptr [rsp + 0x48], xmm2
1804bac15 movsd    xmm0, qword ptr [rsi + rax]
1804bac1a subsd    xmm0, xmm9
1804bac1f movsd    qword ptr [rsi + rax], xmm0
1804bac24 addsd    xmm1, qword ptr [rsi + 0x2a8]
1804bac2c movsd    qword ptr [rsi + 0x2a8], xmm1
1804bac34 cmp      byte ptr [rsi + 0x2c], 1
1804bac38 movaps   xmm0, xmm9
1804bac3c mulsd    xmm0, xmm8
1804bac41 sete     al
1804bac44 mov      byte ptr [rsp + 0x54], 5
1804bac49 add      al, 9
1804bac4b mov      byte ptr [rsp + 0x48], al
1804bac4f mulsd    xmm0, xmm12
1804bac54 movsd    qword ptr [rsp + 0x4c], xmm0
1804bac5a test     r12, r12
1804bac5d je       0x1804bb455
1804bac63 movups   xmm0, xmmword ptr [rsp + 0x48]
1804bac68 mov      r8, qword ptr [rip + 0x2ce7969]
1804bac6f lea      rdx, [rbp - 0x50]
1804bac73 mov      rcx, r12
1804bac76 movaps   xmmword ptr [rbp - 0x40], xmm2
1804bac7a movaps   xmmword ptr [rbp - 0x50], xmm0
1804bac7e movaps   xmmword ptr [rbp - 0x30], xmm2
1804bac82 call     0x180d08aa0 ; 
1804bac87 mov      rcx, qword ptr [rsi + 0x2a0]
1804bac8e test     rcx, rcx
1804bac91 je       0x1804bb455
1804bac97 movups   xmm0, xmmword ptr [r15 + 0xc]
1804bac9c lea      r9, [rbp - 0x80]
1804baca0 mov      r8, rsi
1804baca3 movups   xmm2, xmmword ptr [r15 + 0x1c]
1804baca8 mov      qword ptr [rsp + 0x20], 0
1804bacb1 movaps   xmm1, xmm9
1804bacb5 movaps   xmmword ptr [rbp - 0x80], xmm0
1804bacb9 movaps   xmmword ptr [rbp - 0x70], xmm2
1804bacbd call     0x180494140 ; 9631:ifapp.Game.Data.GameplayHistory._CMA
1804bacc2 cmp      byte ptr [rsi + 0x2c], 1
1804bacc6 je       0x1804bad09
1804bacc8 cmp      byte ptr [rsi + 0x2c], 2
1804baccc jne      0x1804bb092
1804bacd2 movsd    xmm1, qword ptr [rsi + 0x268]
1804bacda movsd    xmm0, qword ptr [rsi + 0x258]
1804bace2 subsd    xmm1, qword ptr [rsi + 0x260]
1804bacea subsd    xmm0, qword ptr [rsi + 0x250]
1804bacf2 subsd    xmm1, xmm0
1804bacf6 comisd   xmm1, xmm13
1804bacfb setae    al
1804bacfe mov      byte ptr [rsi + 0x270], al
1804bad04 jmp      0x1804bb092 ; 
1804bad09 comisd   xmm11, xmmword ptr [rsi + 0x278]
1804bad12 setae    al
1804bad15 or       byte ptr [rsi + 0x298], al
1804bad1b comisd   xmm11, xmmword ptr [rsi + 0x288]
1804bad24 setae    al
1804bad27 or       byte ptr [rsi + 0x299], al
1804bad2d jmp      0x1804bb092 ; 
1804bad32 mov      ecx, 0x1fa1
1804bad37 cmp      ax, cx
1804bad3a jne      0x1804bb092
1804bad40 cmp      dword ptr [r15 + 8], 0
1804bad45 jne      0x1804bad61
1804bad47 test     rsi, rsi
1804bad4a je       0x1804bb455
1804bad50 cmp      qword ptr [rsi + 0x10], 0
1804bad55 je       0x1804bb455
1804bad5b mov      rax, qword ptr [rsi + 0x10]
1804bad5f jmp      0x1804bad79 ; 
1804bad61 test     rsi, rsi
1804bad64 je       0x1804bb455
1804bad6a cmp      qword ptr [rsi + 0x18], 0
1804bad6f je       0x1804bb455
1804bad75 mov      rax, qword ptr [rsi + 0x18]
1804bad79 movsd    xmm6, qword ptr [rax + 0x40]
1804bad7e cmp      byte ptr [rsi + 0x2c], 1
1804bad82 xorps    xmm7, xmm7
1804bad85 cvtsi2sd xmm7, qword ptr [r15 + 0x24]
1804bad8b mulsd    xmm7, xmm12
1804bad90 je       0x1804bae7a
1804bad96 cmp      byte ptr [rsi + 0x2c], 2
1804bad9a jne      0x1804bb092
1804bada0 cmp      dword ptr [r15 + 8], 0
1804bada5 jne      0x1804badbb
1804bada7 movsd    xmm6, qword ptr [rsi + 0x258]
1804badaf mov      edi, 0x2b8
1804badb4 mov      ebx, 0x250
1804badb9 jmp      0x1804badcd ; 
1804badbb movsd    xmm6, qword ptr [rsi + 0x268]
1804badc3 mov      edi, 0x2c0
1804badc8 mov      ebx, 0x260
1804badcd movsd    xmm8, qword ptr [rbx + rsi]
1804badd3 addsd    xmm7, xmm8
1804badd8 movsd    qword ptr [rbx + rsi], xmm7
1804baddd mov      rcx, qword ptr [rip + 0x2d0b8dc]
1804bade4 cmp      dword ptr [rcx + 0xe4], 0
1804badeb jne      0x1804badf2
1804baded call     0x180309de0 ; 
1804badf2 xor      r8d, r8d
1804badf5 movaps   xmm1, xmm6
1804badf8 movaps   xmm0, xmm7
1804badfb call     0x1818f7200 ; 266:System.Math.Min
1804bae00 movsd    qword ptr [rbx + rsi], xmm0
1804bae05 movaps   xmm3, xmm0
1804bae08 subsd    xmm3, xmm8
1804bae0d movaps   xmm0, xmm3
1804bae10 addsd    xmm0, qword ptr [rdi + rsi]
1804bae15 movsd    qword ptr [rdi + rsi], xmm0
1804bae1a cmp      dword ptr [r15 + 8], 0
1804bae1f mov      rcx, qword ptr [rsi + 0x2a0]
1804bae26 je       0x1804bae60
1804bae28 test     rcx, rcx
1804bae2b je       0x1804bb455
1804bae31 movups   xmm0, xmmword ptr [r15 + 0xc]
1804bae36 lea      r9, [rbp - 0x80]
1804bae3a mov      r8, rsi
1804bae3d movups   xmm2, xmmword ptr [r15 + 0x1c]
1804bae42 mov      qword ptr [rsp + 0x20], 0
1804bae4b movaps   xmm1, xmm3
1804bae4e movaps   xmmword ptr [rbp - 0x80], xmm0
1804bae52 movaps   xmmword ptr [rbp - 0x70], xmm2
1804bae56 call     0x180495260 ; 9631:ifapp.Game.Data.GameplayHistory._eMA
1804bae5b jmp      0x1804bb092 ; 
1804bae60 test     rcx, rcx
1804bae63 je       0x1804bb455
1804bae69 movups   xmm1, xmmword ptr [r15 + 0x1c]
1804bae6e movaps   xmmword ptr [rbp - 0x70], xmm1
1804bae72 movaps   xmm1, xmm3
1804bae75 jmp      0x1804bb074 ; 
1804bae7a cmp      dword ptr [r15 + 8], 0
1804bae7f jne      0x1804bae93
1804bae81 xor      edx, edx
1804bae83 lea      rcx, [rsi + 0x278]
1804bae8a call     0x180497180 ; 7178:System.Data.AggregateNode.HasRemoteAggregate | 9576:_K._Dh._CLA
1804bae8f test     al, al
1804bae91 jne      0x1804baeb4
1804bae93 cmp      dword ptr [r15 + 8], 1
1804bae98 jne      0x1804bb01a
1804bae9e lea      rcx, [rsi + 0x278]
1804baea5 xor      edx, edx
1804baea7 call     0x180497190 ; 9576:_K._Dh._dLA
1804baeac test     al, al
1804baeae je       0x1804bb01a
1804baeb4 cmp      dword ptr [r15 + 8], 0
1804baeb9 mov      eax, 0x278
1804baebe mov      ecx, 0x288
1804baec3 cmovne   eax, ecx
1804baec6 mov      ebx, eax
1804baec8 jne      0x1804baeda
1804baeca movsd    xmm9, qword ptr [rsi + 0x280]
1804baed3 mov      edi, 0x2b8
1804baed8 jmp      0x1804baee8 ; 
1804baeda movsd    xmm9, qword ptr [rsi + 0x290]
1804baee3 mov      edi, 0x2c0
1804baee8 movsd    xmm10, qword ptr [rsi + rax]
1804baeee movaps   xmm8, xmm7
1804baef2 addsd    xmm8, qword ptr [rbx + rsi]
1804baef8 movsd    qword ptr [rbx + rsi], xmm8
1804baefe mov      rcx, qword ptr [rip + 0x2d0b7bb]
1804baf05 cmp      dword ptr [rcx + 0xe4], 0
1804baf0c jne      0x1804baf13
1804baf0e call     0x180309de0 ; 
1804baf13 xor      r8d, r8d
1804baf16 movaps   xmm1, xmm9
1804baf1a movaps   xmm0, xmm8
1804baf1e call     0x1818f7200 ; 266:System.Math.Min
1804baf23 movsd    qword ptr [rbx + rsi], xmm0
1804baf28 movaps   xmm3, xmm0
1804baf2b subsd    xmm3, xmm10
1804baf30 movaps   xmm0, xmm3
1804baf33 addsd    xmm0, qword ptr [rdi + rsi]
1804baf38 movsd    qword ptr [rdi + rsi], xmm0
1804baf3d cmp      dword ptr [r15 + 8], 0
1804baf42 mov      rcx, qword ptr [rsi + 0x2a0]
1804baf49 je       0x1804bafd5
1804baf4f test     rcx, rcx
1804baf52 je       0x1804bb455
1804baf58 movups   xmm0, xmmword ptr [r15 + 0xc]
1804baf5d lea      r9, [rbp - 0x80]
1804baf61 mov      r8, rsi
1804baf64 movups   xmm2, xmmword ptr [r15 + 0x1c]
1804baf69 mov      qword ptr [rsp + 0x20], 0
1804baf72 movaps   xmm1, xmm3
1804baf75 movaps   xmmword ptr [rbp - 0x80], xmm0
1804baf79 movaps   xmmword ptr [rbp - 0x70], xmm2
1804baf7d call     0x180495260 ; 9631:ifapp.Game.Data.GameplayHistory._eMA
1804baf82 xorps    xmm1, xmm1
1804baf85 movups   xmmword ptr [rsp + 0x48], xmm1
1804baf8a mov      byte ptr [rsp + 0x48], 0xa
1804baf8f mulsd    xmm6, xmm7
1804baf93 mov      byte ptr [rsp + 0x54], 4
1804baf98 mulsd    xmm6, xmm12
1804baf9d movsd    qword ptr [rsp + 0x4c], xmm6
1804bafa3 test     r12, r12
1804bafa6 je       0x1804bb455
1804bafac movups   xmm0, xmmword ptr [rsp + 0x48]
1804bafb1 mov      r8, qword ptr [rip + 0x2ce7620]
1804bafb8 lea      rdx, [rbp - 0x50]
1804bafbc mov      rcx, r12
1804bafbf movaps   xmmword ptr [rbp - 0x40], xmm1
1804bafc3 movaps   xmmword ptr [rbp - 0x50], xmm0
1804bafc7 movaps   xmmword ptr [rbp - 0x30], xmm1
1804bafcb call     0x180d08aa0 ; 
1804bafd0 jmp      0x1804bb092 ; 
1804bafd5 test     rcx, rcx
1804bafd8 je       0x1804bb455
1804bafde movups   xmm1, xmmword ptr [r15 + 0x1c]
1804bafe3 lea      r9, [rbp - 0x80]
1804bafe7 mov      r8, rsi
1804bafea movups   xmm0, xmmword ptr [r15 + 0xc]
1804bafef mov      qword ptr [rsp + 0x20], 0
1804baff8 movaps   xmmword ptr [rbp - 0x70], xmm1
1804baffc movaps   xmm1, xmm3
1804bafff movaps   xmmword ptr [rbp - 0x80], xmm0
1804bb003 call     0x1804942c0 ; 9631:ifapp.Game.Data.GameplayHistory._DMA
1804bb008 xorps    xmm1, xmm1
1804bb00b movups   xmmword ptr [rsp + 0x48], xmm1
1804bb010 mov      byte ptr [rsp + 0x48], 9
1804bb015 jmp      0x1804baf8f ; 
1804bb01a cmp      dword ptr [r15 + 8], 0
1804bb01f mov      rcx, qword ptr [rsi + 0x2a0]
1804bb026 je       0x1804bb05e
1804bb028 test     rcx, rcx
1804bb02b je       0x1804bb455
1804bb031 movups   xmm1, xmmword ptr [r15 + 0x1c]
1804bb036 lea      r9, [rbp - 0x80]
1804bb03a mov      r8, rsi
1804bb03d movups   xmm0, xmmword ptr [r15 + 0xc]
1804bb042 mov      qword ptr [rsp + 0x20], 0
1804bb04b movaps   xmmword ptr [rbp - 0x70], xmm1
1804bb04f movaps   xmm1, xmm11
1804bb053 movaps   xmmword ptr [rbp - 0x80], xmm0
1804bb057 call     0x180495260 ; 9631:ifapp.Game.Data.GameplayHistory._eMA
1804bb05c jmp      0x1804bb092 ; 
1804bb05e test     rcx, rcx
1804bb061 je       0x1804bb455
1804bb067 movups   xmm1, xmmword ptr [r15 + 0x1c]
1804bb06c movaps   xmmword ptr [rbp - 0x70], xmm1
1804bb070 movaps   xmm1, xmm11
1804bb074 movups   xmm0, xmmword ptr [r15 + 0xc]
1804bb079 lea      r9, [rbp - 0x80]
1804bb07d mov      qword ptr [rsp + 0x20], 0
1804bb086 mov      r8, rsi
1804bb089 movaps   xmmword ptr [rbp - 0x80], xmm0
1804bb08d call     0x1804942c0 ; 9631:ifapp.Game.Data.GameplayHistory._DMA
1804bb092 movzx    eax, byte ptr [r15 + 0x14]
1804bb097 xorps    xmm0, xmm0
1804bb09a movsd    xmm1, qword ptr [r15 + 0x24]
1804bb0a0 movups   xmmword ptr [rsp + 0x48], xmm0
1804bb0a5 mov      byte ptr [rsp + 0x48], 0x13
1804bb0aa mov      byte ptr [rsp + 0x54], al
1804bb0ae movups   xmmword ptr [rsp + 0x58], xmm0
1804bb0b3 movups   xmmword ptr [rsp + 0x68], xmm0
1804bb0b8 movsd    xmm0, qword ptr [r15 + 0xc]
1804bb0be movsd    qword ptr [rsp + 0x4c], xmm0
1804bb0c4 movups   xmm0, xmmword ptr [r15 + 0x15]
1804bb0c9 movups   xmmword ptr [rsp + 0x55], xmm0
1804bb0ce movsd    qword ptr [rsp + 0x64], xmm1
1804bb0d4 test     r12, r12
1804bb0d7 je       0x1804bb455
1804bb0dd movups   xmm0, xmmword ptr [rsp + 0x48]
1804bb0e2 mov      r8, qword ptr [rip + 0x2ce74ef]
1804bb0e9 lea      rdx, [rbp - 0x50]
1804bb0ed movups   xmm1, xmmword ptr [rsp + 0x58]
1804bb0f2 mov      rcx, r12
1804bb0f5 movaps   xmmword ptr [rbp - 0x50], xmm0
1804bb0f9 movups   xmm0, xmmword ptr [rsp + 0x68]
1804bb0fe movaps   xmmword ptr [rbp - 0x40], xmm1
1804bb102 movaps   xmmword ptr [rbp - 0x30], xmm0
1804bb106 call     0x180d08aa0 ; 
1804bb10b inc      r13d
1804bb10e mov      dword ptr [rsp + 0x44], r13d
1804bb113 mov      r15, qword ptr [rbp + 0xc0]
1804bb11a jmp      0x1804ba710 ; 
1804bb11f cmp      dword ptr [r15 + 8], 0
1804bb124 jne      0x1804bb141
1804bb126 test     rsi, rsi
1804bb129 je       0x1804bb455
1804bb12f mov      rcx, qword ptr [rsi + 0x10]
1804bb133 test     rcx, rcx
1804bb136 je       0x1804bb455
1804bb13c mov      rax, rcx
1804bb13f jmp      0x1804bb15a ; 
1804bb141 test     rsi, rsi
1804bb144 je       0x1804bb455
1804bb14a mov      rcx, qword ptr [rsi + 0x18]
1804bb14e test     rcx, rcx
1804bb151 je       0x1804bb455
1804bb157 mov      rax, rcx
1804bb15a mov      rdi, qword ptr [rax + 0x88]
1804bb161 mov      rax, qword ptr [rcx + 0x90]
1804bb168 test     rax, rax
1804bb16b je       0x1804bb455
1804bb171 test     rdi, rdi
1804bb174 je       0x1804bb455
1804bb17a xor      ebx, ebx
1804bb17c mov      byte ptr [rsp + 0x40], r14b
1804bb181 cmp      ebx, dword ptr [rdi + 0x18]
1804bb184 jge      0x1804bb1e0
1804bb186 mov      rax, qword ptr [rbp + 0xb0]
1804bb18d movups   xmm0, xmmword ptr [rax]
1804bb190 movaps   xmmword ptr [rbp - 0x80], xmm0
1804bb194 jae      0x1804bb45b
1804bb19a mov      eax, dword ptr [r15 + 0xc]
1804bb19e lea      rcx, [rbp - 0x80]
1804bb1a2 mov      r9d, dword ptr [r15 + 8]
1804bb1a6 mov      r8d, ebx
1804bb1a9 mov      qword ptr [rsp + 0x30], 0
1804bb1b2 mov      dword ptr [rsp + 0x28], eax
1804bb1b6 mov      rax, qword ptr [rbp + 0xc0]
1804bb1bd movsxd   rdx, ebx
1804bb1c0 shl      rdx, 6
1804bb1c4 add      rdx, 0x20
1804bb1c8 mov      qword ptr [rsp + 0x20], rax
1804bb1cd add      rdx, rdi
1804bb1d0 call     0x18049d450 ; 9626:_K._sh._WmA
1804bb1d5 or       al, r14b
1804bb1d8 setne    r14b
1804bb1dc inc      ebx
1804bb1de jmp      0x1804bb17c ; 
1804bb1e0 mov      rcx, qword ptr [rsi + 0x2a0]
1804bb1e7 test     rcx, rcx
1804bb1ea je       0x1804bb455
1804bb1f0 mov      r8d, dword ptr [r15 + 8]
1804bb1f4 mov      r9, rsi
1804bb1f7 mov      edx, dword ptr [r15 + 0xc]
1804bb1fb mov      qword ptr [rsp + 0x20], 0
1804bb204 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804bb209 xorps    xmm1, xmm1
1804bb20c movups   xmmword ptr [rsp + 0x48], xmm1
1804bb211 mov      byte ptr [rsp + 0x48], 0x16
1804bb216 jmp      0x1804bb315 ; 
1804bb21b cmp      dword ptr [r15 + 8], 0
1804bb220 jne      0x1804bb23d
1804bb222 test     rsi, rsi
1804bb225 je       0x1804bb455
1804bb22b mov      rcx, qword ptr [rsi + 0x10]
1804bb22f test     rcx, rcx
1804bb232 je       0x1804bb455
1804bb238 mov      rax, rcx
1804bb23b jmp      0x1804bb256 ; 
1804bb23d test     rsi, rsi
1804bb240 je       0x1804bb455
1804bb246 mov      rcx, qword ptr [rsi + 0x18]
1804bb24a test     rcx, rcx
1804bb24d je       0x1804bb455
1804bb253 mov      rax, rcx
1804bb256 mov      rdi, qword ptr [rax + 0x88]
1804bb25d mov      rax, qword ptr [rcx + 0x90]
1804bb264 test     rax, rax
1804bb267 je       0x1804bb455
1804bb26d nop      dword ptr [rax]
1804bb270 test     rdi, rdi
1804bb273 je       0x1804bb455
1804bb279 xor      ebx, ebx
1804bb27b mov      byte ptr [rsp + 0x40], r14b
1804bb280 cmp      ebx, dword ptr [rdi + 0x18]
1804bb283 jge      0x1804bb2df
1804bb285 mov      rax, qword ptr [rbp + 0xb0]
1804bb28c movups   xmm0, xmmword ptr [rax]
1804bb28f movaps   xmmword ptr [rbp - 0x80], xmm0
1804bb293 jae      0x1804bb45b
1804bb299 mov      eax, dword ptr [r15 + 0xc]
1804bb29d lea      rcx, [rbp - 0x80]
1804bb2a1 mov      r9d, dword ptr [r15 + 8]
1804bb2a5 mov      r8d, ebx
1804bb2a8 mov      qword ptr [rsp + 0x30], 0
1804bb2b1 mov      dword ptr [rsp + 0x28], eax
1804bb2b5 mov      rax, qword ptr [rbp + 0xc0]
1804bb2bc movsxd   rdx, ebx
1804bb2bf shl      rdx, 6
1804bb2c3 add      rdx, 0x20
1804bb2c7 mov      qword ptr [rsp + 0x20], rax
1804bb2cc add      rdx, rdi
1804bb2cf call     0x18049e330 ; 9626:_K._sh._xmA
1804bb2d4 or       al, r14b
1804bb2d7 setne    r14b
1804bb2db inc      ebx
1804bb2dd jmp      0x1804bb27b ; 
1804bb2df mov      rcx, qword ptr [rsi + 0x2a0]
1804bb2e6 test     rcx, rcx
1804bb2e9 je       0x1804bb455
1804bb2ef mov      r8d, dword ptr [r15 + 8]
1804bb2f3 mov      r9, rsi
1804bb2f6 mov      edx, dword ptr [r15 + 0xc]
1804bb2fa mov      qword ptr [rsp + 0x20], 0
1804bb303 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804bb308 xorps    xmm1, xmm1
1804bb30b movups   xmmword ptr [rsp + 0x48], xmm1
1804bb310 mov      byte ptr [rsp + 0x48], 0x17
1804bb315 mov      eax, dword ptr [r15 + 0xc]
1804bb319 mov      dword ptr [rsp + 0x4c], eax
1804bb31d test     r12, r12
1804bb320 je       0x1804bb455
1804bb326 movups   xmm0, xmmword ptr [rsp + 0x48]
1804bb32b mov      r8, qword ptr [rip + 0x2ce72a6]
1804bb332 lea      rdx, [rbp - 0x50]
1804bb336 mov      rcx, r12
1804bb339 movaps   xmmword ptr [rbp - 0x40], xmm1
1804bb33d movaps   xmmword ptr [rbp - 0x50], xmm0
1804bb341 movaps   xmmword ptr [rbp - 0x30], xmm1
1804bb345 call     0x180d08aa0 ; 
1804bb34a inc      r13d
1804bb34d mov      dword ptr [rsp + 0x44], r13d
1804bb352 test     r14b, r14b
1804bb355 jne      0x1804bb363
1804bb357 mov      r15, qword ptr [rbp + 0xc0]
1804bb35e jmp      0x1804ba710 ; 
1804bb363 mov      r15, qword ptr [rbp + 0xc0]
1804bb36a jmp      0x1804ba6a9 ; 
1804bb36f xor      edi, edi
1804bb371 xor      eax, eax
1804bb373 cmp      eax, dword ptr [r15 + 0x18]
1804bb377 jge      0x1804bb3d8
1804bb379 test     rsi, rsi
1804bb37c je       0x1804bb455
1804bb382 mov      rcx, qword ptr [rsi + 0x2e0]
1804bb389 test     rcx, rcx
1804bb38c je       0x1804bb455
1804bb392 mov      rdx, qword ptr [rip + 0x2cba837]
1804bb399 call     0x180fa8800 ; 
1804bb39e mov      r8, qword ptr [rip + 0x2cba8c3]
1804bb3a5 mov      edx, edi
1804bb3a7 mov      rcx, r15
1804bb3aa mov      rbx, rax
1804bb3ad call     0x180fac2f0 ; 
1804bb3b2 inc      edi
1804bb3b4 movups   xmm0, xmmword ptr [rax]
1804bb3b7 movups   xmm1, xmmword ptr [rax + 0x10]
1804bb3bb movups   xmm2, xmmword ptr [rax + 0x20]
1804bb3bf movsd    xmm3, qword ptr [rax + 0x30]
1804bb3c4 mov      eax, edi
1804bb3c6 movups   xmmword ptr [rbx], xmm0
1804bb3c9 movups   xmmword ptr [rbx + 0x10], xmm1
1804bb3cd movups   xmmword ptr [rbx + 0x20], xmm2
1804bb3d1 movsd    qword ptr [rbx + 0x30], xmm3
1804bb3d6 jmp      0x1804bb373 ; 
1804bb3d8 movaps   xmm14, xmmword ptr [rsp + 0xe0]
1804bb3e1 movaps   xmm13, xmmword ptr [rsp + 0xf0]
1804bb3ea movaps   xmm12, xmmword ptr [rsp + 0x100]
1804bb3f3 movaps   xmm11, xmmword ptr [rsp + 0x110]
1804bb3fc movaps   xmm10, xmmword ptr [rsp + 0x120]
1804bb405 movaps   xmm9, xmmword ptr [rsp + 0x130]
1804bb40e movaps   xmm8, xmmword ptr [rsp + 0x140]
1804bb417 movaps   xmm7, xmmword ptr [rsp + 0x150]
1804bb41f movaps   xmm6, xmmword ptr [rsp + 0x160]
1804bb427 mov      r14, qword ptr [rsp + 0x170]
1804bb42f mov      r13, qword ptr [rsp + 0x178]
1804bb437 mov      rdi, qword ptr [rsp + 0x180]
1804bb43f mov      rbx, qword ptr [rsp + 0x1b8]
1804bb447 add      rsp, 0x188
1804bb44e pop      r15
1804bb450 pop      r12
1804bb452 pop      rsi
1804bb453 pop      rbp
1804bb454 ret      
1804bb455 call     0x180309d40 ; 
1804bb45b call     0x180309d30 ; 

FUNCTION 9636 _Vh _XMA 0x1804b9030 3504
1804b9030 push     rsi
1804b9032 sub      rsp, 0x1c0
1804b9039 cmp      byte ptr [rip + 0x2e845c3], 0
1804b9040 mov      rsi, rcx
1804b9043 jne      0x1804b90b8
1804b9045 lea      rcx, [rip + 0x2cd058c]
1804b904c call     0x180309af0 ; 
1804b9051 lea      rcx, [rip + 0x2d0b660]
1804b9058 call     0x180309af0 ; 
1804b905d lea      rcx, [rip + 0x2d0b6ec]
1804b9064 call     0x180309af0 ; 
1804b9069 lea      rcx, [rip + 0x2d0b778]
1804b9070 call     0x180309af0 ; 
1804b9075 lea      rcx, [rip + 0x2d0b804]
1804b907c call     0x180309af0 ; 
1804b9081 lea      rcx, [rip + 0x2d0d638]
1804b9088 call     0x180309af0 ; 
1804b908d lea      rcx, [rip + 0x2d0349c]
1804b9094 call     0x180309af0 ; 
1804b9099 lea      rcx, [rip + 0x2cbd088]
1804b90a0 call     0x180309af0 ; 
1804b90a5 lea      rcx, [rip + 0x2cf631c]
1804b90ac call     0x180309af0 ; 
1804b90b1 mov      byte ptr [rip + 0x2e8454b], 1
1804b90b8 mov      qword ptr [rsp + 0x1d8], rbx
1804b90c0 mov      qword ptr [rsp + 0x1b8], rbp
1804b90c8 mov      qword ptr [rsp + 0x1b0], rdi
1804b90d0 mov      qword ptr [rsp + 0x1a8], r12
1804b90d8 mov      qword ptr [rsp + 0x1a0], r13
1804b90e0 mov      qword ptr [rsp + 0x198], r14
1804b90e8 mov      qword ptr [rsp + 0x190], r15
1804b90f0 movaps   xmmword ptr [rsp + 0x180], xmm6
1804b90f8 movaps   xmmword ptr [rsp + 0x170], xmm7
1804b9100 movaps   xmmword ptr [rsp + 0x160], xmm8
1804b9109 movaps   xmmword ptr [rsp + 0x150], xmm9
1804b9112 movaps   xmmword ptr [rsp + 0x140], xmm10
1804b911b movaps   xmmword ptr [rsp + 0x130], xmm11
1804b9124 movaps   xmmword ptr [rsp + 0x120], xmm12
1804b912d movaps   xmmword ptr [rsp + 0x110], xmm13
1804b9136 movaps   xmmword ptr [rsp + 0x100], xmm14
1804b913f movaps   xmmword ptr [rsp + 0xf0], xmm15
1804b9148 test     rsi, rsi
1804b914b je       0x1804b9d5d
1804b9151 movd     xmm8, dword ptr [rsi + 0x1f0]
1804b915a mov      rcx, qword ptr [rsi + 0x10]
1804b915e movsd    xmm14, qword ptr [rip + 0x214efd9]
1804b9167 cvtdq2pd xmm8, xmm8
1804b916c mulsd    xmm8, xmm14
1804b9171 test     rcx, rcx
1804b9174 je       0x1804b9d5d
1804b917a mov      eax, dword ptr [rsi + 0x1f0]
1804b9180 add      eax, 0x64
1804b9183 mov      dword ptr [rcx + 0x70], eax
1804b9186 mov      rax, qword ptr [rsi + 0x18]
1804b918a test     rax, rax
1804b918d je       0x1804b9d5d
1804b9193 mov      dword ptr [rax + 0x70], 0x64
1804b919a mov      rax, qword ptr [rip + 0x2cf6227]
1804b91a1 cmp      dword ptr [rax + 0xe4], 0
1804b91a8 jne      0x1804b91b9
1804b91aa mov      rcx, rax
1804b91ad call     0x180309de0 ; 
1804b91b2 mov      rax, qword ptr [rip + 0x2cf620f]
1804b91b9 mov      rax, qword ptr [rax + 0xb8]
1804b91c0 mov      rcx, qword ptr [rax + 8]
1804b91c4 test     rcx, rcx
1804b91c7 je       0x1804b9d5d
1804b91cd mov      rdx, qword ptr [rip + 0x2d0b57c]
1804b91d4 call     0x181604f10 ; 
1804b91d9 cmp      byte ptr [rsi + 0x21], 0
1804b91dd jbe      0x1804b9268
1804b91e3 xor      ebx, ebx
1804b91e5 nop      word ptr [rax + rax]
1804b91f0 mov      rax, qword ptr [rip + 0x2cf61d1]
1804b91f7 cmp      dword ptr [rax + 0xe4], 0
1804b91fe jne      0x1804b920f
1804b9200 mov      rcx, rax
1804b9203 call     0x180309de0 ; 
1804b9208 mov      rax, qword ptr [rip + 0x2cf61b9]
1804b920f mov      r9, qword ptr [rsi + 0x10]
1804b9213 test     r9, r9
1804b9216 je       0x1804b9d5d
1804b921c mov      r9, qword ptr [r9 + 0x88]
1804b9223 test     r9, r9
1804b9226 je       0x1804b9d5d
1804b922c cmp      ebx, dword ptr [r9 + 0x18]
1804b9230 jae      0x1804b9dd5
1804b9236 mov      rax, qword ptr [rax + 0xb8]
1804b923d mov      rcx, qword ptr [rax + 8]
1804b9241 test     rcx, rcx
1804b9244 je       0x1804b9d5d
1804b924a mov      r8, qword ptr [rip + 0x2d0b467]
1804b9251 mov      edx, ebx
1804b9253 shl      rdx, 6
1804b9257 mov      edx, dword ptr [r9 + rdx + 0x2c]
1804b925c call     0x1816030f0 ; 
1804b9261 inc      ebx
1804b9263 cmp      ebx, 5
1804b9266 jl       0x1804b91f0
1804b9268 mov      rcx, qword ptr [rip + 0x2cf6159]
1804b926f cmp      dword ptr [rcx + 0xe4], 0
1804b9276 jne      0x1804b9284
1804b9278 call     0x180309de0 ; 
1804b927d mov      rcx, qword ptr [rip + 0x2cf6144]
1804b9284 mov      rax, qword ptr [rcx + 0xb8]
1804b928b mov      rbx, qword ptr [rax + 8]
1804b928f test     rbx, rbx
1804b9292 je       0x1804b9d5d
1804b9298 mov      r8, qword ptr [rip + 0x2d0b549]
1804b929f xor      edx, edx
1804b92a1 mov      rcx, qword ptr [rax + 8]
1804b92a5 mov      ebx, dword ptr [rbx + 0x20]
1804b92a8 call     0x1816061d0 ; 
1804b92ad mov      rcx, qword ptr [rsi + 0x10]
1804b92b1 test     rcx, rcx
1804b92b4 je       0x1804b9d5d
1804b92ba movzx    eax, al
1804b92bd sub      ebx, eax
1804b92bf mov      dword ptr [rcx + 0x68], ebx
1804b92c2 mov      rcx, qword ptr [rsi + 0x10]
1804b92c6 test     rcx, rcx
1804b92c9 je       0x1804b9d5d
1804b92cf lea      eax, [rbx + 0x14]
1804b92d2 lea      eax, [rax + rax*4]
1804b92d5 mov      dword ptr [rcx + 0x6c], eax
1804b92d8 mov      rax, qword ptr [rsi + 0x18]
1804b92dc test     rax, rax
1804b92df je       0x1804b9d5d
1804b92e5 mov      dword ptr [rax + 0x68], 0
1804b92ec mov      rax, qword ptr [rsi + 0x18]
1804b92f0 test     rax, rax
1804b92f3 je       0x1804b9d5d
1804b92f9 movd     xmm1, ebx
1804b92fd cvtdq2pd xmm1, xmm1
1804b9301 mov      dword ptr [rax + 0x6c], 0x64
1804b9308 mov      rax, qword ptr [rsi + 0x10]
1804b930c mulsd    xmm1, qword ptr [rip + 0x214f184]
1804b9314 test     rax, rax
1804b9317 je       0x1804b9d5d
1804b931d mov      byte ptr [rax + 0x80], 0
1804b9324 mov      rax, qword ptr [rsi + 0x18]
1804b9328 test     rax, rax
1804b932b je       0x1804b9d5d
1804b9331 xorps    xmm11, xmm11
1804b9335 mov      byte ptr [rax + 0x80], 0
1804b933c xorps    xmm0, xmm0
1804b933f movsd    qword ptr [rsp + 0x1e8], xmm11
1804b9349 xorps    xmm12, xmm12
1804b934d movsd    qword ptr [rsp + 0x28], xmm0
1804b9353 xorps    xmm7, xmm7
1804b9356 movsd    qword ptr [rsp + 0x30], xmm12
1804b935d movsd    xmm12, qword ptr [rip + 0x214edfa]
1804b9366 xorps    xmm2, xmm2
1804b9369 addsd    xmm8, xmm12
1804b936e movsd    qword ptr [rsp + 0x58], xmm7
1804b9374 movsd    xmm7, qword ptr [rip + 0x214f1dc]
1804b937c addsd    xmm1, xmm12
1804b9381 xorps    xmm6, xmm6
1804b9384 movsd    qword ptr [rsp + 0x68], xmm11
1804b938b xorps    xmm10, xmm10
1804b938f movsd    qword ptr [rsp + 0x20], xmm6
1804b9395 movsd    qword ptr [rsp + 0x38], xmm2
1804b939b xor      r12d, r12d
1804b939e movsd    qword ptr [rsp + 0x90], xmm8
1804b93a8 movsd    qword ptr [rsp + 0x88], xmm1
1804b93b1 movsd    qword ptr [rsp + 0x70], xmm11
1804b93b8 movsd    qword ptr [rsp + 0x40], xmm0
1804b93be movsd    qword ptr [rsp + 0x78], xmm11
1804b93c5 movsd    qword ptr [rsp + 0x48], xmm2
1804b93cb movsd    qword ptr [rsp + 0x80], xmm11
1804b93d5 movsd    qword ptr [rsp + 0x50], xmm0
1804b93db movsd    qword ptr [rsp + 0x60], xmm10
1804b93e2 mov      rax, qword ptr [rsi + 0x10]
1804b93e6 test     rax, rax
1804b93e9 je       0x1804b9d5d
1804b93ef mov      rax, qword ptr [rax + 0x88]
1804b93f6 mov      qword ptr [rsp + 0x1e0], rax
1804b93fe test     rax, rax
1804b9401 je       0x1804b9d5d
1804b9407 cmp      r12d, dword ptr [rax + 0x18]
1804b940b jae      0x1804b9dd5
1804b9411 mov      rax, qword ptr [rsi + 0x10]
1804b9415 mov      rcx, qword ptr [rax + 0x90]
1804b941c test     rcx, rcx
1804b941f je       0x1804b9d5d
1804b9425 cmp      r12d, dword ptr [rcx + 0x18]
1804b9429 jae      0x1804b9dd5
1804b942f mov      rax, qword ptr [rsi + 0x18]
1804b9433 test     rax, rax
1804b9436 je       0x1804b9d5d
1804b943c mov      r13, qword ptr [rax + 0x88]
1804b9443 test     r13, r13
1804b9446 je       0x1804b9d5d
1804b944c cmp      r12d, dword ptr [r13 + 0x18]
1804b9450 jae      0x1804b9dd5
1804b9456 mov      r8, qword ptr [rax + 0x90]
1804b945d test     r8, r8
1804b9460 je       0x1804b9d5d
1804b9466 cmp      r12d, dword ptr [r8 + 0x18]
1804b946a jae      0x1804b9dd5
1804b9470 mov      eax, r12d
1804b9473 add      rax, rax
1804b9476 mov      rdx, qword ptr [rcx + rax*8 + 0x20]
1804b947b test     rdx, rdx
1804b947e je       0x1804b9d5d
1804b9484 movsd    xmm13, qword ptr [rip + 0x214ed03]
1804b948d lea      rcx, [rsp + 0xd0]
1804b9495 mov      eax, r12d
1804b9498 movaps   xmm8, xmm12
1804b949c add      rax, rax
1804b949f movaps   xmm9, xmm12
1804b94a3 movaps   xmm10, xmm12
1804b94a7 movaps   xmm6, xmm12
1804b94ab mov      r15, qword ptr [r8 + rax*8 + 0x20]
1804b94b0 mov      r8, qword ptr [rip + 0x2cbcc71]
1804b94b7 mov      eax, r12d
1804b94ba shl      rax, 6
1804b94be mov      qword ptr [rsp + 0x1d0], rax
1804b94c6 call     0x180fa5990 ; 
1804b94cb mov      r14d, dword ptr [rsp + 0xd8]
1804b94d3 xor      ebp, ebp
1804b94d5 mov      rdi, qword ptr [rsp + 0xd0]
1804b94dd test     r14d, r14d
1804b94e0 jle      0x1804b95fc
1804b94e6 cmp      ebp, r14d
1804b94e9 jae      0x1804b9dd5
1804b94ef lea      rbx, [rbp*4]
1804b94f7 add      rbx, rbp
1804b94fa movzx    edx, word ptr [rdi + rbx*8]
1804b94fe mov      ecx, edx
1804b9500 sub      ecx, 1
1804b9503 je       0x1804b95dd
1804b9509 sub      ecx, 1
1804b950c je       0x1804b95c7
1804b9512 sub      ecx, 1
1804b9515 je       0x1804b9551
1804b9517 sub      ecx, 1
1804b951a je       0x1804b9588
1804b951c cmp      ecx, 1
1804b951f je       0x1804b9568
1804b9521 mov      eax, 0x80
1804b9526 cmp      dx, ax
1804b9529 jne      0x1804b9543
1804b952b xorps    xmm0, xmm0
1804b952e cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b9535 divsd    xmm0, xmm7
1804b9539 addsd    xmm10, xmm0
1804b953e jmp      0x1804b95f1 ; 
1804b9543 mov      eax, 0x81
1804b9548 cmp      dx, ax
1804b954b jne      0x1804b9d63
1804b9551 xorps    xmm0, xmm0
1804b9554 cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b955b divsd    xmm0, xmm7
1804b955f addsd    xmm6, xmm0
1804b9563 jmp      0x1804b95f1 ; 
1804b9568 comisd   xmm11, xmm13
1804b956d jbe      0x1804b9573
1804b956f movaps   xmm13, xmm11
1804b9573 xorps    xmm0, xmm0
1804b9576 cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b957d divsd    xmm0, xmm7
1804b9581 addsd    xmm13, xmm0
1804b9586 jmp      0x1804b95f1 ; 
1804b9588 mov      edx, dword ptr [rsi + 0x1cc]
1804b958e xor      r8d, r8d
1804b9591 mov      rcx, rsi
1804b9594 call     0x1804b80f0 ; 9634:_K._Uh._qMA
1804b9599 cmp      r12d, eax
1804b959c jne      0x1804b95f1
1804b959e mov      rcx, qword ptr [rsp + 0x1d0]
1804b95a6 mov      eax, dword ptr [rdi + rbx*8 + 0x20]
1804b95aa cmp      dword ptr [rcx + r13 + 0x2c], eax
1804b95af je       0x1804b95f1
1804b95b1 mov      rax, qword ptr [rsi + 0x10]
1804b95b5 test     rax, rax
1804b95b8 je       0x1804b9d5d
1804b95be mov      byte ptr [rax + 0x80], 1
1804b95c5 jmp      0x1804b95f1 ; 
1804b95c7 xorps    xmm0, xmm0
1804b95ca cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b95d1 mulsd    xmm0, xmm14
1804b95d6 addsd    xmm9, xmm0
1804b95db jmp      0x1804b95f1 ; 
1804b95dd xorps    xmm0, xmm0
1804b95e0 cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b95e7 mulsd    xmm0, xmm14
1804b95ec addsd    xmm8, xmm0
1804b95f1 inc      ebp
1804b95f3 cmp      ebp, r14d
1804b95f6 jl       0x1804b94e9
1804b95fc mov      rcx, qword ptr [rip + 0x2ccffd5]
1804b9603 mov      rbp, qword ptr [rsp + 0x1e0]
1804b960b mov      rax, qword ptr [rsp + 0x1d0]
1804b9613 mov      r14d, r12d
1804b9616 shl      r14, 6
1804b961a cmp      dword ptr [rcx + 0xe4], 0
1804b9621 mov      edi, dword ptr [rax + r13 + 0x2c]
1804b9626 mov      ebx, dword ptr [r14 + rbp + 0x2c]
1804b962b jne      0x1804b9632
1804b962d call     0x180309de0 ; 
1804b9632 xor      r8d, r8d
1804b9635 mov      edx, edi
1804b9637 mov      ecx, ebx
1804b9639 call     0x18048a8f0 ; 9543:ifapp.Game.Data.CardInstance.GetMultiplier
1804b963e movsd    xmm2, qword ptr [rsp + 0x88]
1804b9647 movaps   xmm1, xmm0
1804b964a movsd    xmm3, qword ptr [rsp + 0x90]
1804b9653 movsd    xmm15, qword ptr [r14 + rbp + 0x30]
1804b965a addsd    xmm15, xmm11
1804b965f divsd    xmm8, xmm10
1804b9664 divsd    xmm9, xmm6
1804b9669 mulsd    xmm15, xmm8
1804b966e mulsd    xmm15, xmm2
1804b9673 mulsd    xmm15, xmm0
1804b9678 movsd    xmm0, qword ptr [r14 + rbp + 0x38]
1804b967f addsd    xmm0, xmm11
1804b9684 mulsd    xmm15, xmm3
1804b9689 mulsd    xmm0, xmm9
1804b968e mulsd    xmm0, xmm2
1804b9692 mulsd    xmm0, xmm1
1804b9696 mulsd    xmm0, xmm3
1804b969a movsd    qword ptr [rsp + 0xa8], xmm0
1804b96a3 movaps   xmm0, xmm2
1804b96a6 mulsd    xmm0, qword ptr [r14 + rbp + 0x30]
1804b96ad mulsd    xmm0, xmm1
1804b96b1 mulsd    xmm0, xmm3
1804b96b5 movsd    qword ptr [rsp + 0xb0], xmm0
1804b96be movaps   xmm0, xmm2
1804b96c1 mulsd    xmm0, qword ptr [r14 + rbp + 0x38]
1804b96c8 movaps   xmm2, xmm1
1804b96cb mulsd    xmm2, qword ptr [r14 + rbp + 0x30]
1804b96d2 mulsd    xmm0, xmm1
1804b96d6 mulsd    xmm1, qword ptr [r14 + rbp + 0x38]
1804b96dd mulsd    xmm0, xmm3
1804b96e1 movsd    qword ptr [rsp + 0xb8], xmm0
1804b96ea movaps   xmm0, xmm3
1804b96ed subsd    xmm0, xmm12
1804b96f2 mulsd    xmm2, xmm0
1804b96f6 movaps   xmm0, xmm3
1804b96f9 subsd    xmm0, xmm12
1804b96fe movsd    qword ptr [rsp + 0xc0], xmm2
1804b9707 mulsd    xmm1, xmm0
1804b970b movsd    qword ptr [rsp + 0xc8], xmm1
1804b9714 test     r15, r15
1804b9717 je       0x1804b9d5d
1804b971d movsd    xmm0, qword ptr [r14 + rbp + 0x30]
1804b9724 lea      rcx, [rsp + 0xe0]
1804b972c mov      r8, qword ptr [rip + 0x2cbc9f5]
1804b9733 mov      rdx, r15
1804b9736 movsd    xmm6, qword ptr [rip + 0x214ea22]
1804b973e movaps   xmm9, xmm12
1804b9742 movsd    xmm8, qword ptr [rip + 0x214ea45]
1804b974b movaps   xmm10, xmm12
1804b974f movsd    qword ptr [rsp + 0x98], xmm0
1804b9758 movsd    xmm0, qword ptr [r14 + rbp + 0x38]
1804b975f movsd    qword ptr [rsp + 0xa0], xmm0
1804b9768 call     0x180fa5990 ; 
1804b976d mov      r15d, dword ptr [rsp + 0xe8]
1804b9775 xor      ebp, ebp
1804b9777 mov      rdi, qword ptr [rsp + 0xe0]
1804b977f test     r15d, r15d
1804b9782 jle      0x1804b98a6
1804b9788 nop      dword ptr [rax + rax]
1804b9790 cmp      ebp, r15d
1804b9793 jae      0x1804b9dd5
1804b9799 lea      rbx, [rbp*4]
1804b97a1 add      rbx, rbp
1804b97a4 movzx    edx, word ptr [rdi + rbx*8]
1804b97a8 mov      ecx, edx
1804b97aa sub      ecx, 1
1804b97ad je       0x1804b9887
1804b97b3 sub      ecx, 1
1804b97b6 je       0x1804b9871
1804b97bc sub      ecx, 1
1804b97bf je       0x1804b97fb
1804b97c1 sub      ecx, 1
1804b97c4 je       0x1804b9832
1804b97c6 cmp      ecx, 1
1804b97c9 je       0x1804b9812
1804b97cb mov      eax, 0x80
1804b97d0 cmp      dx, ax
1804b97d3 jne      0x1804b97ed
1804b97d5 xorps    xmm0, xmm0
1804b97d8 cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b97df divsd    xmm0, xmm7
1804b97e3 addsd    xmm12, xmm0
1804b97e8 jmp      0x1804b989b ; 
1804b97ed mov      eax, 0x81
1804b97f2 cmp      dx, ax
1804b97f5 jne      0x1804b9d9c
1804b97fb xorps    xmm0, xmm0
1804b97fe cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b9805 divsd    xmm0, xmm7
1804b9809 addsd    xmm6, xmm0
1804b980d jmp      0x1804b989b ; 
1804b9812 comisd   xmm11, xmm8
1804b9817 jbe      0x1804b981d
1804b9819 movaps   xmm8, xmm11
1804b981d xorps    xmm0, xmm0
1804b9820 cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b9827 divsd    xmm0, xmm7
1804b982b addsd    xmm8, xmm0
1804b9830 jmp      0x1804b989b ; 
1804b9832 mov      edx, dword ptr [rsi + 0x1cc]
1804b9838 xor      r8d, r8d
1804b983b mov      rcx, rsi
1804b983e call     0x1804b80f0 ; 9634:_K._Uh._qMA
1804b9843 cmp      r12d, eax
1804b9846 jne      0x1804b989b
1804b9848 mov      rcx, qword ptr [rsp + 0x1e0]
1804b9850 mov      eax, dword ptr [rdi + rbx*8 + 0x20]
1804b9854 cmp      dword ptr [rcx + r14 + 0x2c], eax
1804b9859 je       0x1804b989b
1804b985b mov      rax, qword ptr [rsi + 0x18]
1804b985f test     rax, rax
1804b9862 je       0x1804b9d5d
1804b9868 mov      byte ptr [rax + 0x80], 1
1804b986f jmp      0x1804b989b ; 
1804b9871 xorps    xmm0, xmm0
1804b9874 cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b987b mulsd    xmm0, xmm14
1804b9880 addsd    xmm10, xmm0
1804b9885 jmp      0x1804b989b ; 
1804b9887 xorps    xmm0, xmm0
1804b988a cvtsi2sd xmm0, qword ptr [rdi + rbx*8 + 0x20]
1804b9891 mulsd    xmm0, xmm14
1804b9896 addsd    xmm9, xmm0
1804b989b inc      ebp
1804b989d cmp      ebp, r15d
1804b98a0 jl       0x1804b9793
1804b98a6 movsd    xmm0, qword ptr [rsp + 0x20]
1804b98ac addsd    xmm0, qword ptr [rsp + 0x98]
1804b98b5 mov      rcx, qword ptr [rip + 0x2ccfd1c]
1804b98bc mov      rbp, qword ptr [rsp + 0x1d0]
1804b98c4 mov      rax, qword ptr [rsp + 0x1e0]
1804b98cc cmp      dword ptr [rcx + 0xe4], 0
1804b98d3 mov      ebx, dword ptr [rbp + r13 + 0x2c]
1804b98d8 mov      edi, dword ptr [rax + r14 + 0x2c]
1804b98dd movsd    qword ptr [rsp + 0x20], xmm0
1804b98e3 movsd    xmm0, qword ptr [rsp + 0x30]
1804b98e9 addsd    xmm0, qword ptr [rsp + 0xa0]
1804b98f2 movsd    qword ptr [rsp + 0x30], xmm0
1804b98f8 movsd    xmm0, qword ptr [rsp + 0x1e8]
1804b9901 addsd    xmm0, xmm15
1804b9906 movsd    xmm15, qword ptr [rsp + 0x68]
1804b990d addsd    xmm15, qword ptr [rsp + 0xa8]
1804b9917 movsd    qword ptr [rsp + 0x1e8], xmm0
1804b9920 movsd    xmm0, qword ptr [rsp + 0x28]
1804b9926 addsd    xmm0, qword ptr [rsp + 0xb0]
1804b992f movsd    qword ptr [rsp + 0x68], xmm15
1804b9936 movsd    qword ptr [rsp + 0x28], xmm0
1804b993c movsd    xmm0, qword ptr [rsp + 0x38]
1804b9942 addsd    xmm0, qword ptr [rsp + 0xb8]
1804b994b movsd    qword ptr [rsp + 0x38], xmm0
1804b9951 movsd    xmm0, qword ptr [rsp + 0x58]
1804b9957 addsd    xmm0, qword ptr [rsp + 0xc0]
1804b9960 movsd    qword ptr [rsp + 0x58], xmm0
1804b9966 movsd    xmm0, qword ptr [rsp + 0x60]
1804b996c addsd    xmm0, qword ptr [rsp + 0xc8]
1804b9975 movsd    qword ptr [rsp + 0x60], xmm0
1804b997b jne      0x1804b9982
1804b997d call     0x180309de0 ; 
1804b9982 xor      r8d, r8d
1804b9985 mov      edx, edi
1804b9987 mov      ecx, ebx
1804b9989 call     0x18048a8f0 ; 9543:ifapp.Game.Data.CardInstance.GetMultiplier
1804b998e movsd    xmm4, qword ptr [rsp + 0x78]
1804b9994 movaps   xmm2, xmm0
1804b9997 movsd    xmm0, qword ptr [rsp + 0x40]
1804b999d inc      r12d
1804b99a0 movsd    xmm5, qword ptr [rsp + 0x80]
1804b99a9 movsd    xmm3, qword ptr [rsp + 0x70]
1804b99af movsd    xmm1, qword ptr [rbp + r13 + 0x38]
1804b99b6 addsd    xmm1, xmm11
1804b99bb addsd    xmm0, qword ptr [rbp + r13 + 0x38]
1804b99c2 addsd    xmm3, qword ptr [rbp + r13 + 0x30]
1804b99c9 divsd    xmm10, xmm6
1804b99ce movsd    xmm6, qword ptr [rsp + 0x48]
1804b99d4 movsd    qword ptr [rsp + 0x40], xmm0
1804b99da movsd    xmm0, qword ptr [rbp + r13 + 0x30]
1804b99e1 mulsd    xmm1, xmm10
1804b99e6 addsd    xmm0, xmm11
1804b99eb movsd    qword ptr [rsp + 0x70], xmm3
1804b99f1 divsd    xmm9, xmm12
1804b99f6 mulsd    xmm1, xmm2
1804b99fa mulsd    xmm0, xmm9
1804b99ff addsd    xmm6, xmm1
1804b9a03 mulsd    xmm0, xmm2
1804b9a07 movsd    qword ptr [rsp + 0x48], xmm6
1804b9a0d addsd    xmm4, xmm0
1804b9a11 movaps   xmm0, xmm2
1804b9a14 mulsd    xmm0, qword ptr [rbp + r13 + 0x30]
1804b9a1b mulsd    xmm2, qword ptr [rbp + r13 + 0x38]
1804b9a22 movsd    qword ptr [rsp + 0x78], xmm4
1804b9a28 addsd    xmm5, xmm0
1804b9a2c movsd    xmm0, qword ptr [rsp + 0x50]
1804b9a32 addsd    xmm0, xmm2
1804b9a36 movsd    qword ptr [rsp + 0x80], xmm5
1804b9a3f movsd    qword ptr [rsp + 0x50], xmm0
1804b9a45 cmp      r12d, 5
1804b9a49 jge      0x1804b9a59
1804b9a4b movsd    xmm12, qword ptr [rip + 0x214e70c]
1804b9a54 jmp      0x1804b93e2 ; 
1804b9a59 mov      rax, qword ptr [rsi + 0x10]
1804b9a5d test     rax, rax
1804b9a60 je       0x1804b9d5d
1804b9a66 cmp      byte ptr [rax + 0x80], 0
1804b9a6d je       0x1804b9a7b
1804b9a6f movaps   xmm3, xmm11
1804b9a73 movaps   xmm4, xmm11
1804b9a77 movaps   xmm5, xmm11
1804b9a7b mov      rax, qword ptr [rsi + 0x18]
1804b9a7f test     rax, rax
1804b9a82 je       0x1804b9d5d
1804b9a88 cmp      byte ptr [rax + 0x80], 0
1804b9a8f je       0x1804b9a9f
1804b9a91 movaps   xmm6, xmm11
1804b9a95 movaps   xmm9, xmm11
1804b9a99 movaps   xmm0, xmm11
1804b9a9d jmp      0x1804b9ab5 ; 
1804b9a9f movsd    xmm9, qword ptr [rsp + 0x1e8]
1804b9aa9 movsd    xmm6, qword ptr [rsp + 0x20]
1804b9aaf movsd    xmm0, qword ptr [rsp + 0x28]
1804b9ab5 mov      rax, qword ptr [rsi + 0x10]
1804b9ab9 movsd    qword ptr [rax + 0x18], xmm6
1804b9abe mov      rax, qword ptr [rsi + 0x10]
1804b9ac2 test     rax, rax
1804b9ac5 je       0x1804b9d5d
1804b9acb movsd    xmm12, qword ptr [rsp + 0x30]
1804b9ad2 movsd    qword ptr [rax + 0x20], xmm12
1804b9ad8 mov      rax, qword ptr [rsi + 0x10]
1804b9adc test     rax, rax
1804b9adf je       0x1804b9d5d
1804b9ae5 movsd    qword ptr [rax + 0x28], xmm9
1804b9aeb mov      rax, qword ptr [rsi + 0x10]
1804b9aef test     rax, rax
1804b9af2 je       0x1804b9d5d
1804b9af8 movsd    qword ptr [rax + 0x30], xmm15
1804b9afe mov      rax, qword ptr [rsi + 0x10]
1804b9b02 test     rax, rax
1804b9b05 je       0x1804b9d5d
1804b9b0b movsd    qword ptr [rax + 0x38], xmm0
1804b9b10 mov      rax, qword ptr [rsi + 0x10]
1804b9b14 test     rax, rax
1804b9b17 je       0x1804b9d5d
1804b9b1d movsd    xmm2, qword ptr [rsp + 0x38]
1804b9b23 movsd    qword ptr [rax + 0x40], xmm2
1804b9b28 mov      rax, qword ptr [rsi + 0x18]
1804b9b2c test     rax, rax
1804b9b2f je       0x1804b9d5d
1804b9b35 movsd    qword ptr [rax + 0x18], xmm3
1804b9b3a mov      rax, qword ptr [rsi + 0x18]
1804b9b3e test     rax, rax
1804b9b41 je       0x1804b9d5d
1804b9b47 movsd    xmm0, qword ptr [rsp + 0x40]
1804b9b4d movsd    qword ptr [rax + 0x20], xmm0
1804b9b52 mov      rax, qword ptr [rsi + 0x18]
1804b9b56 test     rax, rax
1804b9b59 je       0x1804b9d5d
1804b9b5f movsd    qword ptr [rax + 0x28], xmm4
1804b9b64 mov      rax, qword ptr [rsi + 0x18]
1804b9b68 test     rax, rax
1804b9b6b je       0x1804b9d5d
1804b9b71 movsd    xmm2, qword ptr [rsp + 0x48]
1804b9b77 movsd    qword ptr [rax + 0x30], xmm2
1804b9b7c mov      rax, qword ptr [rsi + 0x18]
1804b9b80 test     rax, rax
1804b9b83 je       0x1804b9d5d
1804b9b89 movsd    qword ptr [rax + 0x38], xmm5
1804b9b8e mov      rax, qword ptr [rsi + 0x18]
1804b9b92 test     rax, rax
1804b9b95 je       0x1804b9d5d
1804b9b9b movsd    xmm0, qword ptr [rsp + 0x50]
1804b9ba1 movsd    qword ptr [rax + 0x40], xmm0
1804b9ba6 mov      rax, qword ptr [rsi + 0x10]
1804b9baa test     rax, rax
1804b9bad je       0x1804b9d5d
1804b9bb3 movsd    xmm7, qword ptr [rsp + 0x58]
1804b9bb9 movsd    qword ptr [rax + 0x58], xmm7
1804b9bbe mov      rax, qword ptr [rsi + 0x10]
1804b9bc2 test     rax, rax
1804b9bc5 je       0x1804b9d5d
1804b9bcb movsd    xmm10, qword ptr [rsp + 0x60]
1804b9bd2 movsd    qword ptr [rax + 0x60], xmm10
1804b9bd8 mov      rax, qword ptr [rsi + 0x18]
1804b9bdc test     rax, rax
1804b9bdf je       0x1804b9d5d
1804b9be5 xor      edi, edi
1804b9be7 mov      qword ptr [rax + 0x58], rdi
1804b9beb mov      rax, qword ptr [rsi + 0x18]
1804b9bef test     rax, rax
1804b9bf2 je       0x1804b9d5d
1804b9bf8 mov      qword ptr [rax + 0x60], rdi
1804b9bfc mov      rcx, qword ptr [rip + 0x2d0cabd]
1804b9c03 mov      rbx, qword ptr [rsi + 0x10]
1804b9c07 cmp      dword ptr [rcx + 0xe4], edi
1804b9c0d jne      0x1804b9c14
1804b9c0f call     0x180309de0 ; 
1804b9c14 subsd    xmm9, xmm6
1804b9c19 xor      r9d, r9d
1804b9c1c movsd    xmm6, qword ptr [rip + 0x214e99c]
1804b9c24 movaps   xmm2, xmm6
1804b9c27 subsd    xmm9, xmm7
1804b9c2c movsd    xmm7, qword ptr [rip + 0x214e9a4]
1804b9c34 movaps   xmm1, xmm7
1804b9c37 movaps   xmm0, xmm9
1804b9c3b call     0x1804a5160 ; 
1804b9c40 test     rbx, rbx
1804b9c43 je       0x1804b9d5d
1804b9c49 subsd    xmm15, xmm12
1804b9c4e movsd    qword ptr [rbx + 0x48], xmm0
1804b9c53 mov      rbx, qword ptr [rsi + 0x10]
1804b9c57 xor      r9d, r9d
1804b9c5a movaps   xmm2, xmm6
1804b9c5d movaps   xmm1, xmm7
1804b9c60 subsd    xmm15, xmm10
1804b9c65 movaps   xmm0, xmm15
1804b9c69 call     0x1804a5160 ; 
1804b9c6e test     rbx, rbx
1804b9c71 je       0x1804b9d5d
1804b9c77 movsd    qword ptr [rbx + 0x50], xmm0
1804b9c7c mov      rax, qword ptr [rsi + 0x18]
1804b9c80 test     rax, rax
1804b9c83 je       0x1804b9d5d
1804b9c89 mov      qword ptr [rax + 0x48], rdi
1804b9c8d mov      rax, qword ptr [rsi + 0x18]
1804b9c91 test     rax, rax
1804b9c94 je       0x1804b9d5d
1804b9c9a mov      qword ptr [rax + 0x50], rdi
1804b9c9e mov      rax, qword ptr [rsi + 0x10]
1804b9ca2 test     rax, rax
1804b9ca5 je       0x1804b9d5d
1804b9cab movsd    qword ptr [rax + 0x78], xmm13
1804b9cb1 mov      rax, qword ptr [rsi + 0x18]
1804b9cb5 test     rax, rax
1804b9cb8 je       0x1804b9d5d
1804b9cbe movaps   xmm15, xmmword ptr [rsp + 0xf0]
1804b9cc7 movaps   xmm14, xmmword ptr [rsp + 0x100]
1804b9cd0 movaps   xmm13, xmmword ptr [rsp + 0x110]
1804b9cd9 movaps   xmm12, xmmword ptr [rsp + 0x120]
1804b9ce2 movaps   xmm11, xmmword ptr [rsp + 0x130]
1804b9ceb movaps   xmm10, xmmword ptr [rsp + 0x140]
1804b9cf4 movaps   xmm9, xmmword ptr [rsp + 0x150]
1804b9cfd movaps   xmm7, xmmword ptr [rsp + 0x170]
1804b9d05 movaps   xmm6, xmmword ptr [rsp + 0x180]
1804b9d0d mov      r15, qword ptr [rsp + 0x190]
1804b9d15 mov      r14, qword ptr [rsp + 0x198]
1804b9d1d mov      r13, qword ptr [rsp + 0x1a0]
1804b9d25 mov      r12, qword ptr [rsp + 0x1a8]
1804b9d2d mov      rdi, qword ptr [rsp + 0x1b0]
1804b9d35 mov      rbp, qword ptr [rsp + 0x1b8]
1804b9d3d mov      rbx, qword ptr [rsp + 0x1d8]
1804b9d45 movsd    qword ptr [rax + 0x78], xmm8
1804b9d4b movaps   xmm8, xmmword ptr [rsp + 0x160]
1804b9d54 add      rsp, 0x1c0
1804b9d5b pop      rsi
1804b9d5c ret      
1804b9d5d call     0x180309d40 ; 
1804b9d63 lea      rcx, [rip + 0x2cc4c3e]
1804b9d6a call     0x180309b10 ; 
1804b9d6f mov      rcx, rax
1804b9d72 call     0x180309ce0 ; 
1804b9d77 xor      edx, edx
1804b9d79 mov      rcx, rax
1804b9d7c mov      rbx, rax
1804b9d7f call     0x1818c3000 ; 181:System.ArgumentOutOfRangeException..ctor
1804b9d84 lea      rcx, [rip + 0x2cba9c5]
1804b9d8b call     0x180309b10 ; 
1804b9d90 mov      rdx, rax
1804b9d93 mov      rcx, rbx
1804b9d96 call     0x180309d00 ; 
1804b9d9c lea      rcx, [rip + 0x2cc4c05]
1804b9da3 call     0x180309b10 ; 
1804b9da8 mov      rcx, rax
1804b9dab call     0x180309ce0 ; 
1804b9db0 xor      edx, edx
1804b9db2 mov      rcx, rax
1804b9db5 mov      rbx, rax
1804b9db8 call     0x1818c3000 ; 181:System.ArgumentOutOfRangeException..ctor
1804b9dbd lea      rcx, [rip + 0x2cba98c]
1804b9dc4 call     0x180309b10 ; 
1804b9dc9 mov      rdx, rax
1804b9dcc mov      rcx, rbx
1804b9dcf call     0x180309d00 ; 
1804b9dd5 call     0x180309d30 ; 

FUNCTION 9636 _Vh .cctor 0x1804b9de0 192
1804b9de0 push     rbx
1804b9de2 sub      rsp, 0x20
1804b9de6 cmp      byte ptr [rip + 0x2e83817], 0
1804b9ded jne      0x1804b9e32
1804b9def lea      rcx, [rip + 0x2cb1e2a]
1804b9df6 call     0x180309af0 ; 
1804b9dfb lea      rcx, [rip + 0x2d0a81e]
1804b9e02 call     0x180309af0 ; 
1804b9e07 lea      rcx, [rip + 0x2cd4dba]
1804b9e0e call     0x180309af0 ; 
1804b9e13 lea      rcx, [rip + 0x2ce57c6]
1804b9e1a call     0x180309af0 ; 
1804b9e1f lea      rcx, [rip + 0x2cf55a2]
1804b9e26 call     0x180309af0 ; 
1804b9e2b mov      byte ptr [rip + 0x2e837d2], 1
1804b9e32 mov      rcx, qword ptr [rip + 0x2cb1de7]
1804b9e39 mov      edx, 4
1804b9e3e call     0x180308ef0 ; 
1804b9e43 mov      rdx, qword ptr [rip + 0x2ce5796]
1804b9e4a xor      r8d, r8d
1804b9e4d mov      rcx, rax
1804b9e50 mov      rbx, rax
1804b9e53 call     0x181811ef0 ; 1075:System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray
1804b9e58 mov      rcx, qword ptr [rip + 0x2cf5569]
1804b9e5f mov      rdx, qword ptr [rcx + 0xb8]
1804b9e66 mov      qword ptr [rdx], rbx
1804b9e69 mov      rcx, qword ptr [rip + 0x2cd4d58]
1804b9e70 call     0x180309ce0 ; 
1804b9e75 mov      rdx, qword ptr [rip + 0x2d0a7a4]
1804b9e7c mov      rcx, rax
1804b9e7f mov      rbx, rax
1804b9e82 call     0x181616dc0 ; 
1804b9e87 mov      rcx, qword ptr [rip + 0x2cf553a]
1804b9e8e mov      rdx, qword ptr [rcx + 0xb8]
1804b9e95 mov      qword ptr [rdx + 8], rbx
1804b9e99 add      rsp, 0x20
1804b9e9d pop      rbx
1804b9e9e ret      

FUNCTION 9634 _Uh .ctor 0x1804b7c30 672
1804b7c30 push     rbx
1804b7c32 push     rbp
1804b7c33 push     rsi
1804b7c34 push     rdi
1804b7c35 push     r14
1804b7c37 sub      rsp, 0x1f0
1804b7c3e cmp      byte ptr [rip + 0x2e859b8], 0
1804b7c45 mov      rbp, r9
1804b7c48 mov      r14, r8
1804b7c4b mov      rsi, rdx
1804b7c4e mov      rdi, rcx
1804b7c51 jne      0x1804b7c96
1804b7c53 lea      rcx, [rip + 0x2cb3fc6]
1804b7c5a call     0x180309af0 ; 
1804b7c5f lea      rcx, [rip + 0x2cef172]
1804b7c66 call     0x180309af0 ; 
1804b7c6b lea      rcx, [rip + 0x2cb6b9e]
1804b7c72 call     0x180309af0 ; 
1804b7c77 lea      rcx, [rip + 0x2cbde22]
1804b7c7e call     0x180309af0 ; 
1804b7c83 lea      rcx, [rip + 0x2d1038e]
1804b7c8a call     0x180309af0 ; 
1804b7c8f mov      byte ptr [rip + 0x2e85967], 1
1804b7c96 mov      rcx, qword ptr [rip + 0x2cb6b73]
1804b7c9d mov      edx, 5
1804b7ca2 call     0x180308ef0 ; 
1804b7ca7 mov      qword ptr [rdi + 0x238], rax
1804b7cae mov      edx, 5
1804b7cb3 mov      rcx, qword ptr [rip + 0x2cb6b56]
1804b7cba call     0x180308ef0 ; 
1804b7cbf mov      qword ptr [rdi + 0x240], rax
1804b7cc6 mov      edx, 5
1804b7ccb mov      rcx, qword ptr [rip + 0x2cb3f4e]
1804b7cd2 call     0x180308ef0 ; 
1804b7cd7 mov      qword ptr [rdi + 0x248], rax
1804b7cde mov      rcx, qword ptr [rip + 0x2d10333]
1804b7ce5 call     0x180309ce0 ; 
1804b7cea mov      r8, qword ptr [rip + 0x2cbddaf]
1804b7cf1 mov      edx, 0x20
1804b7cf6 mov      rcx, rax
1804b7cf9 mov      rbx, rax
1804b7cfc call     0x180fa5800 ; 
1804b7d01 movabs   rax, 0x402e000000000000
1804b7d0b mov      qword ptr [rdi + 0x2e0], rbx
1804b7d12 xor      edx, edx
1804b7d14 mov      qword ptr [rdi + 0x2e8], rax
1804b7d1b mov      rcx, rdi
1804b7d1e mov      dword ptr [rdi + 0x2f0], 0x5dc
1804b7d28 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b7d2d mov      r8d, 3
1804b7d33 lea      rcx, [rsp + 0x58]
1804b7d38 mov      edx, r8d
1804b7d3b mov      rax, rsi
1804b7d3e nop      
1804b7d40 lea      rcx, [rcx + 0x80]
1804b7d47 movups   xmm0, xmmword ptr [rax]
1804b7d4a movups   xmm1, xmmword ptr [rax + 0x10]
1804b7d4e lea      rax, [rax + 0x80]
1804b7d55 movups   xmmword ptr [rcx - 0x80], xmm0
1804b7d59 movups   xmm0, xmmword ptr [rax - 0x60]
1804b7d5d movups   xmmword ptr [rcx - 0x70], xmm1
1804b7d61 movups   xmm1, xmmword ptr [rax - 0x50]
1804b7d65 movups   xmmword ptr [rcx - 0x60], xmm0
1804b7d69 movups   xmm0, xmmword ptr [rax - 0x40]
1804b7d6d movups   xmmword ptr [rcx - 0x50], xmm1
1804b7d71 movups   xmm1, xmmword ptr [rax - 0x30]
1804b7d75 movups   xmmword ptr [rcx - 0x40], xmm0
1804b7d79 movups   xmm0, xmmword ptr [rax - 0x20]
1804b7d7d movups   xmmword ptr [rcx - 0x30], xmm1
1804b7d81 movups   xmm1, xmmword ptr [rax - 0x10]
1804b7d85 movups   xmmword ptr [rcx - 0x20], xmm0
1804b7d89 movups   xmmword ptr [rcx - 0x10], xmm1
1804b7d8d sub      rdx, 1
1804b7d91 jne      0x1804b7d40
1804b7d93 movups   xmm0, xmmword ptr [rax]
1804b7d96 mov      rax, qword ptr [rax + 0x10]
1804b7d9a movups   xmmword ptr [rcx], xmm0
1804b7d9d mov      qword ptr [rcx + 0x10], rax
1804b7da1 lea      rcx, [rdi + 0x30]
1804b7da5 lea      rax, [rsp + 0x58]
1804b7daa nop      word ptr [rax + rax]
1804b7db0 lea      rcx, [rcx + 0x80]
1804b7db7 movups   xmm0, xmmword ptr [rax]
1804b7dba movups   xmm1, xmmword ptr [rax + 0x10]
1804b7dbe lea      rax, [rax + 0x80]
1804b7dc5 movups   xmmword ptr [rcx - 0x80], xmm0
1804b7dc9 movups   xmm0, xmmword ptr [rax - 0x60]
1804b7dcd movups   xmmword ptr [rcx - 0x70], xmm1
1804b7dd1 movups   xmm1, xmmword ptr [rax - 0x50]
1804b7dd5 movups   xmmword ptr [rcx - 0x60], xmm0
1804b7dd9 movups   xmm0, xmmword ptr [rax - 0x40]
1804b7ddd movups   xmmword ptr [rcx - 0x50], xmm1
1804b7de1 movups   xmm1, xmmword ptr [rax - 0x30]
1804b7de5 movups   xmmword ptr [rcx - 0x40], xmm0
1804b7de9 movups   xmm0, xmmword ptr [rax - 0x20]
1804b7ded movups   xmmword ptr [rcx - 0x30], xmm1
1804b7df1 movups   xmm1, xmmword ptr [rax - 0x10]
1804b7df5 movups   xmmword ptr [rcx - 0x20], xmm0
1804b7df9 movups   xmmword ptr [rcx - 0x10], xmm1
1804b7dfd sub      r8, 1
1804b7e01 jne      0x1804b7db0
1804b7e03 movups   xmm0, xmmword ptr [rax]
1804b7e06 mov      rax, qword ptr [rax + 0x10]
1804b7e0a mov      qword ptr [rdi + 0x10], r14
1804b7e0e movups   xmmword ptr [rcx], xmm0
1804b7e11 mov      qword ptr [rcx + 0x10], rax
1804b7e15 mov      rcx, qword ptr [rsp + 0x240]
1804b7e1d mov      qword ptr [rdi + 0x18], rbp
1804b7e21 mov      eax, dword ptr [rcx]
1804b7e23 mov      dword ptr [rdi + 0x20], eax
1804b7e26 movzx    eax, word ptr [rcx + 4]
1804b7e2a mov      word ptr [rdi + 0x24], ax
1804b7e2e movzx    eax, byte ptr [rcx + 6]
1804b7e32 mov      byte ptr [rdi + 0x26], al
1804b7e35 movzx    eax, byte ptr [rsp + 0x248]
1804b7e3d mov      byte ptr [rdi + 0x27], al
1804b7e40 mov      rcx, qword ptr [rip + 0x2ceef91]
1804b7e47 call     0x180309ce0 ; 
1804b7e4c xor      edx, edx
1804b7e4e mov      rcx, rax
1804b7e51 mov      rbx, rax
1804b7e54 call     0x180494c50 ; 9631:ifapp.Game.Data.GameplayHistory..ctor
1804b7e59 movsd    xmm2, qword ptr [rip + 0x21502ff]
1804b7e61 xor      eax, eax
1804b7e63 mov      qword ptr [rdi + 0x2a0], rbx
1804b7e6a xorps    xmm0, xmm0
1804b7e6d cmp      dword ptr [rsi + 4], 1
1804b7e71 movsd    qword ptr [rsp + 0x40], xmm2
1804b7e77 mov      qword ptr [rsp + 0x20], rax
1804b7e7c movdqu   xmmword ptr [rsp + 0x28], xmm0
1804b7e82 mov      qword ptr [rsp + 0x38], rax
1804b7e87 jne      0x1804b7e91
1804b7e89 movsd    xmm2, qword ptr [rip + 0x21502c7]
1804b7e91 movups   xmm0, xmmword ptr [rsp + 0x20]
1804b7e96 movups   xmm1, xmmword ptr [rsp + 0x30]
1804b7e9b movups   xmmword ptr [rdi + 0x2a8], xmm0
1804b7ea2 movups   xmm0, xmmword ptr [rsp + 0x40]
1804b7ea7 movups   xmmword ptr [rdi + 0x2b8], xmm1
1804b7eae unpcklpd xmm0, xmm2
1804b7eb2 movups   xmmword ptr [rdi + 0x2c8], xmm0
1804b7eb9 mov      qword ptr [rdi + 0x2d8], rax
1804b7ec0 add      rsp, 0x1f0
1804b7ec7 pop      r14
1804b7ec9 pop      rdi
1804b7eca pop      rsi
1804b7ecb pop      rbp
1804b7ecc pop      rbx
1804b7ecd ret      

FUNCTION 9634 _Uh _oMA 0x1804b7ed0 32
1804b7ed0 mov      eax, 0x51eb851f
1804b7ed5 imul     dword ptr [rcx + 0x2f0]
1804b7edb sar      edx, 5
1804b7ede mov      eax, edx
1804b7ee0 shr      eax, 0x1f
1804b7ee3 add      eax, edx
1804b7ee5 ret      

FUNCTION 9634 _Uh _OMA 0x1804b78c0 48
1804b78c0 mov      rdx, rcx
1804b78c3 xorps    xmm0, xmm0
1804b78c6 movups   xmmword ptr [rcx + 0x218], xmm0
1804b78cd xor      r8d, r8d
1804b78d0 movups   xmmword ptr [rcx + 0x226], xmm0
1804b78d7 mov      rdx, qword ptr [rdx + 0x240]
1804b78de add      rcx, 0x218
1804b78e5 jmp      0x1804b76a0 ; 9708:_K._DH._RnA

FUNCTION 9634 _Uh _pMA 0x1804b7ef0 512
1804b7ef0 mov      qword ptr [rsp + 8], rbx
1804b7ef5 mov      qword ptr [rsp + 0x10], rsi
1804b7efa push     rdi
1804b7efb sub      rsp, 0x30
1804b7eff cmp      byte ptr [rip + 0x2e856f8], 0
1804b7f06 mov      rdi, rcx
1804b7f09 movaps   xmmword ptr [rsp + 0x20], xmm7
1804b7f0e movzx    ebx, r8w
1804b7f12 movzx    esi, dl
1804b7f15 jne      0x1804b7f2a
1804b7f17 lea      rcx, [rip + 0x2d0e7a2]
1804b7f1e call     0x180309af0 ; 
1804b7f23 mov      byte ptr [rip + 0x2e856d4], 1
1804b7f2a lea      eax, [rbx - 1]
1804b7f2d lea      r8, [rip - 0x4b7f34]
1804b7f34 cmp      eax, 8
1804b7f37 ja       0x1804b7f8e
1804b7f39 lea      eax, [rbx - 1]
1804b7f3c movsxd   rcx, eax
1804b7f3f mov      eax, dword ptr [r8 + rcx*4 + 0x4b80a8]
1804b7f47 add      rax, r8
1804b7f4a jmp      rax
1804b7f4c mov      ecx, 0x222
1804b7f51 jmp      0x1804b7f6d ; 
1804b7f53 mov      ecx, 0x224
1804b7f58 jmp      0x1804b7f6d ; 
1804b7f5a mov      ecx, 0x226
1804b7f5f jmp      0x1804b7f6d ; 
1804b7f61 mov      ecx, 0x228
1804b7f66 jmp      0x1804b7f6d ; 
1804b7f68 mov      ecx, 0x22a
1804b7f6d lea      eax, [rsi + 0xfc]
1804b7f73 test     al, 0xfd
1804b7f75 jne      0x1804b7f7e
1804b7f77 mov      edx, 1
1804b7f7c jmp      0x1804b7f8a ; 
1804b7f7e xor      eax, eax
1804b7f80 cmp      sil, 5
1804b7f84 sete     al
1804b7f87 movzx    edx, ax
1804b7f8a add      word ptr [rcx + rdi], dx
1804b7f8e movd     xmm0, dword ptr [rdi + 0x1c8]
1804b7f96 lea      eax, [rsi - 1]
1804b7f99 movsd    xmm1, qword ptr [rip + 0x21505b7]
1804b7fa1 cvtdq2pd xmm0, xmm0
1804b7fa5 divsd    xmm1, xmm0
1804b7fa9 cmp      eax, 5
1804b7fac ja       0x1804b7ff3
1804b7fae lea      eax, [rsi - 1]
1804b7fb1 movsxd   rcx, eax
1804b7fb4 mov      eax, dword ptr [r8 + rcx*4 + 0x4b80cc]
1804b7fbc add      rax, r8
1804b7fbf jmp      rax
1804b7fc1 movsd    xmm0, qword ptr [rdi + 0x2e8]
1804b7fc9 subsd    xmm0, qword ptr [rip + 0x215018f]
1804b7fd1 movsd    qword ptr [rdi + 0x2e8], xmm0
1804b7fd9 jmp      0x1804b7ff3 ; 
1804b7fdb mulsd    xmm1, qword ptr [rip + 0x2150175]
1804b7fe3 addsd    xmm1, qword ptr [rdi + 0x2e8]
1804b7feb movsd    qword ptr [rdi + 0x2e8], xmm1
1804b7ff3 mov      rcx, qword ptr [rip + 0x2d0e6c6]
1804b7ffa movsd    xmm7, qword ptr [rdi + 0x2e8]
1804b8002 cmp      dword ptr [rcx + 0xe4], 0
1804b8009 jne      0x1804b8010
1804b800b call     0x180309de0 ; 
1804b8010 movsd    xmm2, qword ptr [rip + 0x2150598]
1804b8018 xor      r9d, r9d
1804b801b movsd    xmm1, qword ptr [rip + 0x21505ad]
1804b8023 movaps   xmm0, xmm7
1804b8026 call     0x1804a5160 ; 
1804b802b movsd    qword ptr [rdi + 0x2e8], xmm0
1804b8033 mulsd    xmm0, qword ptr [rip + 0x215051d]
1804b803b call     0x1800099f0 ; 
1804b8040 cmp      byte ptr [rip + 0x2e85330], 0
1804b8047 cvttsd2si ebx, xmm0
1804b804b jne      0x1804b806c
1804b804d lea      rcx, [rip + 0x2cc131c]
1804b8054 call     0x180309af0 ; 
1804b8059 lea      rcx, [rip + 0x2d0e660]
1804b8060 call     0x180309af0 ; 
1804b8065 mov      byte ptr [rip + 0x2e8530b], 1
1804b806c cmp      ebx, 0xfffffa24
1804b8072 jl       0x1804b8086
1804b8074 mov      eax, 0x5dc
1804b8079 cmp      ebx, eax
1804b807b cmovle   eax, ebx
1804b807e mov      dword ptr [rdi + 0x2f0], eax
1804b8084 jmp      0x1804b8090 ; 
1804b8086 mov      dword ptr [rdi + 0x2f0], 0xfffffa24
1804b8090 mov      rbx, qword ptr [rsp + 0x40]
1804b8095 mov      rsi, qword ptr [rsp + 0x48]
1804b809a movaps   xmm7, xmmword ptr [rsp + 0x20]
1804b809f add      rsp, 0x30
1804b80a3 pop      rdi
1804b80a4 ret      
1804b80a5 nop      dword ptr [rax]
1804b80a8 jg       0x1804b80f6
1804b80ab add      byte ptr [rsi + 0x53004b7f], cl
1804b80b1 jg       0x1804b80fe
1804b80b3 add      byte ptr [rsi + 0x5a004b7f], cl
1804b80b9 jg       0x1804b8106
1804b80bb add      byte ptr [rsi + 0x61004b7f], cl
1804b80c1 jg       0x1804b810e
1804b80c3 add      byte ptr [rsi + 0x68004b7f], cl
1804b80c9 jg       0x1804b8116
1804b80cb add      cl, al
1804b80cd jg       0x1804b811a
1804b80cf add      bl, bl
1804b80d1 jg       0x1804b811e
1804b80d3 add      bl, bl
1804b80d5 jg       0x1804b8122
1804b80d7 add      bl, ah
1804b80d9 jg       0x1804b8126
1804b80db add      bl, ah
1804b80dd jg       0x1804b812a
1804b80df add      bl, ah
1804b80e1 jg       0x1804b812e
1804b80e3 add      ah, cl

FUNCTION 9634 _Uh _PMA 0x1804b78f0 16
1804b78f0 inc      dword ptr [rcx + 0x1cc]
1804b78f6 ret      

FUNCTION 9634 _Uh _qMA 0x1804b80f0 80
1804b80f0 sub      rsp, 0x28
1804b80f4 mov      r8, qword ptr [rcx + 0x238]
1804b80fb nop      dword ptr [rax + rax]
1804b8100 xor      eax, eax
1804b8102 test     r8, r8
1804b8105 je       0x1804b812f
1804b8107 cmp      eax, dword ptr [r8 + 0x18]
1804b810b jae      0x1804b8135
1804b810d movsxd   rcx, eax
1804b8110 cmp      dword ptr [r8 + rcx*4 + 0x20], edx
1804b8115 jg       0x1804b8128
1804b8117 inc      eax
1804b8119 cmp      eax, 5
1804b811c jl       0x1804b8107
1804b811e mov      eax, 4
1804b8123 add      rsp, 0x28
1804b8127 ret      
1804b8128 dec      eax
1804b812a add      rsp, 0x28
1804b812e ret      
1804b812f call     0x180309d40 ; 
1804b8135 call     0x180309d30 ; 

FUNCTION 9634 _Uh _QMA 0x1804b7900 128
1804b7900 sub      rsp, 0x28
1804b7904 mov      r9, qword ptr [rcx + 0x238]
1804b790b nop      dword ptr [rax + rax]
1804b7910 xor      r8d, r8d
1804b7913 test     r9, r9
1804b7916 je       0x1804b7967
1804b7918 cmp      r8d, dword ptr [r9 + 0x18]
1804b791c jae      0x1804b796d
1804b791e movsxd   rax, r8d
1804b7921 cmp      dword ptr [r9 + rax*4 + 0x20], edx
1804b7926 jg       0x1804b7939
1804b7928 inc      r8d
1804b792b cmp      r8d, 5
1804b792f jl       0x1804b7918
1804b7931 mov      r8d, 4
1804b7937 jmp      0x1804b793f ; 
1804b7939 sub      r8d, 1
1804b793d js       0x1804b795f
1804b793f mov      rcx, qword ptr [rcx + 0x248]
1804b7946 test     rcx, rcx
1804b7949 je       0x1804b7967
1804b794b cmp      r8d, dword ptr [rcx + 0x18]
1804b794f jae      0x1804b796d
1804b7951 mov      eax, r8d
1804b7954 movsd    xmm0, qword ptr [rcx + rax*8 + 0x20]
1804b795a add      rsp, 0x28
1804b795e ret      
1804b795f xorps    xmm0, xmm0
1804b7962 add      rsp, 0x28
1804b7966 ret      
1804b7967 call     0x180309d40 ; 
1804b796d call     0x180309d30 ; 

FUNCTION 9634 _Uh _rMA 0x1804b8140 224
1804b8140 push     rbx
1804b8142 sub      rsp, 0x20
1804b8146 mov      ebx, edx
1804b8148 mov      r11, rcx
1804b814b nop      dword ptr [rax + rax]
1804b8150 xor      eax, eax
1804b8152 test     edx, edx
1804b8154 je       0x1804b81e5
1804b815a mov      r10, qword ptr [rcx + 0x238]
1804b8161 test     r10, r10
1804b8164 je       0x1804b8205
1804b816a nop      word ptr [rax + rax]
1804b8170 cmp      eax, dword ptr [r10 + 0x18]
1804b8174 jae      0x1804b820b
1804b817a movsxd   rcx, eax
1804b817d cmp      ebx, dword ptr [r10 + rcx*4 + 0x20]
1804b8182 jle      0x1804b81b2
1804b8184 test     r10, r10
1804b8187 je       0x1804b8205
1804b8189 mov      r9, qword ptr [r11 + 0x240]
1804b8190 test     r9, r9
1804b8193 je       0x1804b8205
1804b8195 cmp      eax, dword ptr [r9 + 0x18]
1804b8199 jae      0x1804b820b
1804b819b movsxd   rcx, eax
1804b819e movsxd   rdx, eax
1804b81a1 mov      r8d, dword ptr [r10 + rcx*4 + 0x20]
1804b81a6 add      r8d, dword ptr [r9 + rdx*4 + 0x20]
1804b81ab cmp      ebx, r8d
1804b81ae jl       0x1804b81eb
1804b81b0 jmp      0x1804b81c4 ; 
1804b81b2 mov      r9, qword ptr [r11 + 0x240]
1804b81b9 test     r9, r9
1804b81bc je       0x1804b8205
1804b81be cmp      eax, dword ptr [r9 + 0x18]
1804b81c2 jae      0x1804b820b
1804b81c4 movsxd   rcx, eax
1804b81c7 movsxd   rdx, eax
1804b81ca mov      r8d, dword ptr [r10 + rcx*4 + 0x20]
1804b81cf add      r8d, dword ptr [r9 + rdx*4 + 0x20]
1804b81d4 cmp      ebx, r8d
1804b81d7 je       0x1804b81f8
1804b81d9 inc      eax
1804b81db cmp      eax, 5
1804b81de jl       0x1804b8170
1804b81e0 mov      eax, 0xa
1804b81e5 add      rsp, 0x20
1804b81e9 pop      rbx
1804b81ea ret      
1804b81eb add      ax, ax
1804b81ee or       ax, 1
1804b81f2 add      rsp, 0x20
1804b81f6 pop      rbx
1804b81f7 ret      
1804b81f8 add      ax, ax
1804b81fb add      ax, 2
1804b81ff add      rsp, 0x20
1804b8203 pop      rbx
1804b8204 ret      
1804b8205 call     0x180309d40 ; 
1804b820b call     0x180309d30 ; 

FUNCTION 9634 _Uh _RMA 0x1804b7980 144
1804b7980 push     rbx
1804b7982 sub      rsp, 0x20
1804b7986 mov      r9, qword ptr [rcx + 0x238]
1804b798d mov      r11d, edx
1804b7990 mov      rbx, rcx
1804b7993 xor      eax, eax
1804b7995 test     r9, r9
1804b7998 je       0x1804b79fb
1804b799a nop      word ptr [rax + rax]
1804b79a0 cmp      eax, dword ptr [r9 + 0x18]
1804b79a4 jae      0x1804b7a01
1804b79a6 movsxd   rcx, eax
1804b79a9 cmp      r11d, dword ptr [r9 + rcx*4 + 0x20]
1804b79ae jl       0x1804b79dc
1804b79b0 test     r9, r9
1804b79b3 je       0x1804b79fb
1804b79b5 mov      r10, qword ptr [rbx + 0x240]
1804b79bc test     r10, r10
1804b79bf je       0x1804b79fb
1804b79c1 cmp      eax, dword ptr [r10 + 0x18]
1804b79c5 jae      0x1804b7a01
1804b79c7 movsxd   rcx, eax
1804b79ca movsxd   rdx, eax
1804b79cd mov      r8d, dword ptr [r9 + rcx*4 + 0x20]
1804b79d2 add      r8d, dword ptr [r10 + rdx*4 + 0x20]
1804b79d7 cmp      r11d, r8d
1804b79da jl       0x1804b79ee
1804b79dc inc      eax
1804b79de cmp      eax, 5
1804b79e1 jl       0x1804b79a0
1804b79e3 mov      eax, 0xa
1804b79e8 add      rsp, 0x20
1804b79ec pop      rbx
1804b79ed ret      
1804b79ee add      ax, ax
1804b79f1 or       ax, 1
1804b79f5 add      rsp, 0x20
1804b79f9 pop      rbx
1804b79fa ret      
1804b79fb call     0x180309d40 ; 
1804b7a01 call     0x180309d30 ; 

FUNCTION 9634 _Uh _sMA 0x1804b8220 112
1804b8220 sub      rsp, 0x28
1804b8224 mov      r8, qword ptr [rcx + 0x238]
1804b822b mov      r9, rcx
1804b822e nop      
1804b8230 xor      eax, eax
1804b8232 test     r8, r8
1804b8235 je       0x1804b8276
1804b8237 cmp      eax, dword ptr [r8 + 0x18]
1804b823b jae      0x1804b827c
1804b823d movsxd   rcx, eax
1804b8240 cmp      dword ptr [r8 + rcx*4 + 0x20], edx
1804b8245 jg       0x1804b8255
1804b8247 inc      eax
1804b8249 cmp      eax, 5
1804b824c jl       0x1804b8237
1804b824e mov      eax, 4
1804b8253 jmp      0x1804b8257 ; 
1804b8255 dec      eax
1804b8257 movsxd   rdx, eax
1804b825a cdqe     
1804b825c movzx    ecx, word ptr [r9 + rax*2 + 0x218]
1804b8265 cmp      word ptr [r9 + rdx*2 + 0x222], cx
1804b826e setge    al
1804b8271 add      rsp, 0x28
1804b8275 ret      
1804b8276 call     0x180309d40 ; 
1804b827c call     0x180309d30 ; 

FUNCTION 9634 _Uh _SMA 0x1804b7a10 112
1804b7a10 sub      rsp, 0x28
1804b7a14 mov      r8, qword ptr [rcx + 0x238]
1804b7a1b mov      r9, rcx
1804b7a1e nop      
1804b7a20 xor      eax, eax
1804b7a22 test     r8, r8
1804b7a25 je       0x1804b7a6e
1804b7a27 cmp      eax, dword ptr [r8 + 0x18]
1804b7a2b jae      0x1804b7a74
1804b7a2d movsxd   rcx, eax
1804b7a30 cmp      dword ptr [r8 + rcx*4 + 0x20], edx
1804b7a35 jg       0x1804b7a46
1804b7a37 inc      eax
1804b7a39 cmp      eax, 5
1804b7a3c jl       0x1804b7a27
1804b7a3e mov      r8d, 4
1804b7a44 jmp      0x1804b7a4a ; 
1804b7a46 lea      r8d, [rax - 1]
1804b7a4a movsxd   rax, r8d
1804b7a4d movsx    ecx, word ptr [r9 + rax*2 + 0x22c]
1804b7a56 movsxd   rax, r8d
1804b7a59 sub      edx, ecx
1804b7a5b movsx    ecx, word ptr [r9 + rax*2 + 0x218]
1804b7a64 cmp      edx, ecx
1804b7a66 setge    al
1804b7a69 add      rsp, 0x28
1804b7a6d ret      
1804b7a6e call     0x180309d40 ; 
1804b7a74 call     0x180309d30 ; 

FUNCTION 9634 _Uh _tMA 0x1804b8290 272
1804b8290 movzx    eax, dx
1804b8293 lea      r10, [rip - 0x4b829a]
1804b829a dec      eax
1804b829c mov      r9, rcx
1804b829f cmp      eax, 8
1804b82a2 ja       0x1804b82b9
1804b82a4 movzx    eax, dx
1804b82a7 dec      eax
1804b82a9 movsxd   r8, eax
1804b82ac mov      eax, dword ptr [r10 + r8*4 + 0x4b8358]
1804b82b4 add      rax, r10
1804b82b7 jmp      rax
1804b82b9 movzx    eax, dx
1804b82bc sub      eax, 2
1804b82bf cmp      eax, 8
1804b82c2 ja       0x1804b8352
1804b82c8 movzx    eax, dx
1804b82cb sub      eax, 2
1804b82ce movsxd   rcx, eax
1804b82d1 mov      eax, dword ptr [r10 + rcx*4 + 0x4b837c]
1804b82d9 add      rax, r10
1804b82dc jmp      rax
1804b82de mov      ecx, 0x222
1804b82e3 mov      eax, 0x218
1804b82e8 jmp      0x1804b8318 ; 
1804b82ea mov      ecx, 0x224
1804b82ef mov      eax, 0x21a
1804b82f4 jmp      0x1804b8318 ; 
1804b82f6 mov      ecx, 0x226
1804b82fb mov      eax, 0x21c
1804b8300 jmp      0x1804b8318 ; 
1804b8302 mov      ecx, 0x228
1804b8307 mov      eax, 0x21e
1804b830c jmp      0x1804b8318 ; 
1804b830e mov      ecx, 0x22a
1804b8313 mov      eax, 0x220
1804b8318 movsx    eax, word ptr [rax + r9]
1804b831d xorps    xmm0, xmm0
1804b8320 movd     xmm2, eax
1804b8324 cvtdq2ps xmm2, xmm2
1804b8327 comiss   xmm2, xmm0
1804b832a jbe      0x1804b8355
1804b832c movsx    eax, word ptr [rcx + r9]
1804b8331 movd     xmm1, eax
1804b8335 cvtdq2ps xmm1, xmm1
1804b8338 divss    xmm1, xmm2
1804b833c comiss   xmm0, xmm1
1804b833f ja       0x1804b8355
1804b8341 movss    xmm0, dword ptr [rip + 0x214f073]
1804b8349 comiss   xmm1, xmm0
1804b834c ja       0x1804b8355
1804b834e movaps   xmm0, xmm1
1804b8351 ret      
1804b8352 xorps    xmm0, xmm0
1804b8355 ret      
1804b8356 nop      
1804b8358 fiadd    word ptr [rdx - 0x7d46ffb5]
1804b835e add      r10b, bpl

FUNCTION 9634 _Uh _TMA 0x1804b7a80 432
1804b7a80 mov      qword ptr [rsp + 8], rbx
1804b7a85 push     rdi
1804b7a86 sub      rsp, 0x70
1804b7a8a xorps    xmm0, xmm0
1804b7a8d xor      eax, eax
1804b7a8f mov      qword ptr [rsp + 0x60], rax
1804b7a94 mov      rdi, rdx
1804b7a97 mov      byte ptr [rsp + 0x60], 0
1804b7a9c mov      rbx, rcx
1804b7a9f movups   xmmword ptr [rcx], xmm0
1804b7aa2 movups   xmmword ptr [rcx + 0x10], xmm0
1804b7aa6 movups   xmmword ptr [rcx + 0x20], xmm0
1804b7aaa movups   xmmword ptr [rcx + 0x30], xmm0
1804b7aae mov      qword ptr [rcx + 0x40], rax
1804b7ab2 mov      eax, dword ptr [rdx + 0x30]
1804b7ab5 movsd    xmm2, qword ptr [rdx + 0x2d8]
1804b7abd movups   xmm1, xmmword ptr [rdx + 0x2b8]
1804b7ac4 movups   xmmword ptr [rsp + 0x30], xmm0
1804b7ac9 movups   xmmword ptr [rsp + 0x20], xmm0
1804b7ace mov      dword ptr [rsp + 0x20], eax
1804b7ad2 movups   xmmword ptr [rsp + 0x40], xmm0
1804b7ad7 movups   xmmword ptr [rsp + 0x50], xmm0
1804b7adc movups   xmm0, xmmword ptr [rdx + 0x2a8]
1804b7ae3 movups   xmmword ptr [rsp + 0x38], xmm1
1804b7ae8 movups   xmmword ptr [rsp + 0x28], xmm0
1804b7aed movaps   xmm1, xmmword ptr [rsp + 0x30]
1804b7af2 movups   xmm0, xmmword ptr [rdx + 0x2c8]
1804b7af9 movups   xmmword ptr [rsp + 0x48], xmm0
1804b7afe movaps   xmm0, xmmword ptr [rsp + 0x20]
1804b7b03 movups   xmmword ptr [rcx], xmm0
1804b7b06 movaps   xmm0, xmmword ptr [rsp + 0x40]
1804b7b0b movups   xmmword ptr [rcx + 0x10], xmm1
1804b7b0f movaps   xmm1, xmmword ptr [rsp + 0x50]
1804b7b14 unpcklpd xmm1, xmm2
1804b7b18 movups   xmmword ptr [rcx + 0x20], xmm0
1804b7b1c movsd    xmm0, qword ptr [rsp + 0x60]
1804b7b22 movups   xmmword ptr [rcx + 0x30], xmm1
1804b7b26 movsd    qword ptr [rcx + 0x40], xmm0
1804b7b2b test     r8, r8
1804b7b2e je       0x1804b7c2a
1804b7b34 xor      edx, edx
1804b7b36 mov      rcx, r8
1804b7b39 call     0x1804ce010 ; 9751:_K._NH._EoA
1804b7b3e test     al, al
1804b7b40 jne      0x1804b7bfc
1804b7b46 mov      dword ptr [rbx + 0x44], 2
1804b7b4d cmp      byte ptr [rdi + 0x2c], 2
1804b7b51 je       0x1804b7b84
1804b7b53 cmp      byte ptr [rdi + 0x2c], 1
1804b7b57 jne      0x1804b7c19
1804b7b5d xor      eax, eax
1804b7b5f cmp      byte ptr [rdi + 0x299], al
1804b7b65 je       0x1804b7b70
1804b7b67 cmp      byte ptr [rdi + 0x298], al
1804b7b6d sete     al
1804b7b70 mov      byte ptr [rbx + 0x40], al
1804b7b73 mov      rax, rbx
1804b7b76 mov      rbx, qword ptr [rsp + 0x80]
1804b7b7e add      rsp, 0x70
1804b7b82 pop      rdi
1804b7b83 ret      
1804b7b84 movsd    xmm1, qword ptr [rdi + 0x268]
1804b7b8c subsd    xmm1, qword ptr [rdi + 0x260]
1804b7b94 movsd    xmm0, qword ptr [rdi + 0x258]
1804b7b9c subsd    xmm0, qword ptr [rdi + 0x250]
1804b7ba4 movsd    xmm2, qword ptr [rip + 0x21509ac]
1804b7bac subsd    xmm1, xmm0
1804b7bb0 comisd   xmm1, xmm2
1804b7bb4 setae    al
1804b7bb7 mov      byte ptr [rdi + 0x270], al
1804b7bbd movsd    xmm1, qword ptr [rdi + 0x268]
1804b7bc5 movsd    xmm0, qword ptr [rdi + 0x258]
1804b7bcd subsd    xmm1, qword ptr [rdi + 0x260]
1804b7bd5 subsd    xmm0, qword ptr [rdi + 0x250]
1804b7bdd subsd    xmm1, xmm0
1804b7be1 comisd   xmm1, xmm2
1804b7be5 setae    al
1804b7be8 mov      byte ptr [rbx + 0x40], al
1804b7beb mov      rax, rbx
1804b7bee mov      rbx, qword ptr [rsp + 0x80]
1804b7bf6 add      rsp, 0x70
1804b7bfa pop      rdi
1804b7bfb ret      
1804b7bfc xor      eax, eax
1804b7bfe mov      byte ptr [rbx + 0x40], 1
1804b7c02 mov      dword ptr [rbx + 0x44], 1
1804b7c09 mov      qword ptr [rbx + 8], rax
1804b7c0d mov      qword ptr [rbx + 0x10], rax
1804b7c11 mov      qword ptr [rbx + 0x18], rax
1804b7c15 mov      qword ptr [rbx + 0x20], rax
1804b7c19 mov      rax, rbx
1804b7c1c mov      rbx, qword ptr [rsp + 0x80]
1804b7c24 add      rsp, 0x70
1804b7c28 pop      rdi
1804b7c29 ret      
1804b7c2a call     0x180309d40 ; 

FUNCTION 9567 _Ah _qlA 0x180497100 128
180497100 mov      qword ptr [rsp + 8], rbx
180497105 push     rdi
180497106 sub      rsp, 0x20
18049710a cmp      byte ptr [rip + 0x2ea645c], 0
180497111 mov      rdi, rcx
180497114 jne      0x180497135
180497116 lea      rcx, [rip + 0x2cded13]
18049711d call     0x180309af0 ; 
180497122 lea      rcx, [rip + 0x2d30f87]
180497129 call     0x180309af0 ; 
18049712e mov      byte ptr [rip + 0x2ea6438], 1
180497135 mov      rcx, qword ptr [rip + 0x2d30f74]
18049713c xorps    xmm0, xmm0
18049713f movups   xmmword ptr [rdi], xmm0
180497142 call     0x180309ce0 ; 
180497147 mov      r8, qword ptr [rip + 0x2cdece2]
18049714e mov      edx, 8
180497153 mov      rcx, rax
180497156 mov      rbx, rax
180497159 call     0x180fa5800 ; 
18049715e mov      qword ptr [rdi], rbx
180497161 mov      rax, rdi
180497164 mov      rbx, qword ptr [rsp + 0x30]
180497169 mov      word ptr [rdi + 8], 0
18049716f add      rsp, 0x20
180497173 pop      rdi
180497174 ret      

FUNCTION 9567 _Ah _QlA 0x1804970b0 48
1804970b0 mov      r10, rcx
1804970b3 mov      eax, 1
1804970b8 lea      ecx, [r8 + rdx*2]
1804970bc and      ecx, 0x1f
1804970bf shl      al, cl
1804970c1 movzx    edx, byte ptr [r10 + 9]
1804970c6 test     r9b, r9b
1804970c9 je       0x1804970d2
1804970cb or       dl, al
1804970cd mov      byte ptr [r10 + 9], dl
1804970d1 ret      
1804970d2 not      al
1804970d4 and      al, dl
1804970d6 mov      byte ptr [r10 + 9], al
1804970da ret      

FUNCTION 9567 _Ah _RlA 0x1804970e0 32
1804970e0 mov      r9, rcx
1804970e3 mov      eax, 1
1804970e8 lea      ecx, [r8 + rdx*2]
1804970ec and      ecx, 0x1f
1804970ef shl      al, cl
1804970f1 test     byte ptr [r9 + 9], al
1804970f5 seta     al
1804970f8 ret      

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

FUNCTION 9530 BattleScore GetStarResult 0x180489360 592
180489360 mov      qword ptr [rsp + 8], rbx
180489365 push     rdi
180489366 sub      rsp, 0x50
18048936a cmp      byte ptr [rip + 0x2eb41bb], 0
180489371 mov      rdi, rdx
180489374 movaps   xmmword ptr [rsp + 0x40], xmm6
180489379 mov      rbx, rcx
18048937c movaps   xmmword ptr [rsp + 0x30], xmm7
180489381 movaps   xmmword ptr [rsp + 0x20], xmm8
180489387 jne      0x1804893a8
180489389 lea      rcx, [rip + 0x2d3d3c8]
180489390 call     0x180309af0 ; 
180489395 lea      rcx, [rip + 0x2d3d324]
18048939c call     0x180309af0 ; 
1804893a1 mov      byte ptr [rip + 0x2eb4184], 1
1804893a8 xor      eax, eax
1804893aa xorps    xmm6, xmm6
1804893ad cmp      byte ptr [rip + 0x2eb4239], al
1804893b3 cvtsi2ss xmm6, rdi
1804893b8 mov      dword ptr [rbx], eax
1804893ba mov      byte ptr [rbx + 4], al
1804893bd divss    xmm6, dword ptr [rip + 0x217f1ab]
1804893c5 jne      0x1804893da
1804893c7 lea      rcx, [rip + 0x2d3d2f2]
1804893ce call     0x180309af0 ; 
1804893d3 mov      byte ptr [rip + 0x2eb4212], 1
1804893da mov      rcx, qword ptr [rip + 0x2d3d2df]
1804893e1 cmp      dword ptr [rcx + 0xe4], 0
1804893e8 jne      0x1804893ef
1804893ea call     0x180309de0 ; 
1804893ef movsd    xmm1, qword ptr [rip + 0x217f159]
1804893f7 xor      r8d, r8d
1804893fa cvtps2pd xmm0, xmm6
1804893fd call     0x1818f6ed0 ; 266:System.Math.Log
180489402 movss    xmm8, dword ptr [rip + 0x217f035]
18048940b cvtsd2ss xmm0, xmm0
18048940f divss    xmm0, xmm8
180489414 call     0x1803da7d0 ; 
180489419 mov      rcx, qword ptr [rip + 0x2d3d338]
180489420 movaps   xmm6, xmm0
180489423 cmp      dword ptr [rcx + 0xe4], 0
18048942a jne      0x180489431
18048942c call     0x180309de0 ; 
180489431 cmp      byte ptr [rip + 0x2eb41b5], 0
180489438 movss    xmm7, dword ptr [rip + 0x217df7c]
180489440 mulss    xmm6, xmm8
180489445 addss    xmm6, xmm7
180489449 jne      0x18048945e
18048944b lea      rcx, [rip + 0x2d3d26e]
180489452 call     0x180309af0 ; 
180489457 mov      byte ptr [rip + 0x2eb418f], 1
18048945e mov      rcx, qword ptr [rip + 0x2d3d25b]
180489465 cmp      dword ptr [rcx + 0xe4], 0
18048946c jne      0x180489473
18048946e call     0x180309de0 ; 
180489473 xor      r8d, r8d
180489476 xorps    xmm1, xmm1
180489479 movaps   xmm0, xmm6
18048947c call     0x1818f7070 ; 266:System.Math.Max
180489481 mov      rcx, qword ptr [rip + 0x2d3d238]
180489488 movaps   xmm6, xmm0
18048948b cmp      dword ptr [rcx + 0xe4], 0
180489492 jne      0x180489499
180489494 call     0x180309de0 ; 
180489499 movaps   xmm1, xmm7
18048949c movaps   xmm0, xmm6
18048949f call     0x1803d9af0 ; 
1804894a4 cmp      byte ptr [rip + 0x2eb4143], 0
1804894ab divss    xmm0, xmm8
1804894b0 xorps    xmm8, xmm8
1804894b4 cvtss2sd xmm8, xmm0
1804894b9 jne      0x1804894ce
1804894bb lea      rcx, [rip + 0x2d3d1fe]
1804894c2 call     0x180309af0 ; 
1804894c7 mov      byte ptr [rip + 0x2eb4120], 1
1804894ce mov      rcx, qword ptr [rip + 0x2d3d1eb]
1804894d5 cmp      dword ptr [rcx + 0xe4], 0
1804894dc jne      0x1804894e3
1804894de call     0x180309de0 ; 
1804894e3 xor      r9d, r9d
1804894e6 xor      r8d, r8d
1804894e9 mov      edx, 1
1804894ee movaps   xmm0, xmm8
1804894f2 call     0x1818f72f0 ; 266:System.Math.Round
1804894f7 comiss   xmm7, xmm6
1804894fa cvttsd2si eax, xmm0
1804894fe ja       0x18048957c
180489500 movss    xmm0, dword ptr [rip + 0x217ecdc]
180489508 comiss   xmm0, xmm6
18048950b mov      byte ptr [rbx], 5
18048950e ja       0x18048956d
180489510 movss    xmm0, dword ptr [rip + 0x217ecdc]
180489518 comiss   xmm0, xmm6
18048951b ja       0x18048955e
18048951d movss    xmm0, dword ptr [rip + 0x217f03b]
180489525 comiss   xmm0, xmm6
180489528 ja       0x18048954f
18048952a movss    xmm0, dword ptr [rip + 0x217ecc6]
180489532 comiss   xmm0, xmm6
180489535 ja       0x180489540
180489537 mov      dword ptr [rbx + 1], 0x5050505
18048953e jmp      0x180489585 ; 
180489540 mov      word ptr [rbx + 1], 0x505
180489546 mov      byte ptr [rbx + 3], 5
18048954a mov      byte ptr [rbx + 4], al
18048954d jmp      0x180489585 ; 
18048954f mov      word ptr [rbx + 1], 0x505
180489555 mov      byte ptr [rbx + 3], al
180489558 mov      byte ptr [rbx + 4], 0
18048955c jmp      0x180489585 ; 
18048955e mov      byte ptr [rbx + 1], 5
180489562 mov      byte ptr [rbx + 2], al
180489565 mov      word ptr [rbx + 3], 0
18048956b jmp      0x180489585 ; 
18048956d mov      byte ptr [rbx + 1], al
180489570 mov      word ptr [rbx + 2], 0
180489576 mov      byte ptr [rbx + 4], 0
18048957a jmp      0x180489585 ; 
18048957c mov      byte ptr [rbx], al
18048957e mov      dword ptr [rbx + 1], 0
180489585 mov      rax, rbx
180489588 mov      rbx, qword ptr [rsp + 0x60]
18048958d movaps   xmm6, xmmword ptr [rsp + 0x40]
180489592 movaps   xmm7, xmmword ptr [rsp + 0x30]
180489597 movaps   xmm8, xmmword ptr [rsp + 0x20]
18048959d add      rsp, 0x50
1804895a1 pop      rdi
1804895a2 ret      

FUNCTION 9530 BattleScore .cctor 0x180489840 640
180489840 push     rbx
180489842 sub      rsp, 0x20
180489846 cmp      byte ptr [rip + 0x2eb3ce0], 0
18048984d jne      0x180489892
18048984f lea      rcx, [rip + 0x2cf9aaa]
180489856 call     0x180309af0 ; 
18048985b lea      rcx, [rip + 0x2ce23be]
180489862 call     0x180309af0 ; 
180489867 lea      rcx, [rip + 0x2ce4fa2]
18048986e call     0x180309af0 ; 
180489873 lea      rcx, [rip + 0x2d1637e]
18048987a call     0x180309af0 ; 
18048987f lea      rcx, [rip + 0x2d16852]
180489886 call     0x180309af0 ; 
18048988b mov      byte ptr [rip + 0x2eb3c9b], 1
180489892 mov      rcx, qword ptr [rip + 0x2ce4f77]
180489899 mov      edx, 6
18048989e call     0x180308ef0 ; 
1804898a3 mov      rdx, qword ptr [rip + 0x2d1682e]
1804898aa xor      r8d, r8d
1804898ad mov      rcx, rax
1804898b0 mov      rbx, rax
1804898b3 call     0x181811ef0 ; 1075:System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray
1804898b8 mov      rcx, qword ptr [rip + 0x2cf9a41]
1804898bf mov      rdx, qword ptr [rcx + 0xb8]
1804898c6 mov      qword ptr [rdx], rbx
1804898c9 mov      edx, 7
1804898ce mov      rcx, qword ptr [rip + 0x2ce234b]
1804898d5 call     0x180308ef0 ; 
1804898da mov      rdx, qword ptr [rip + 0x2d16317]
1804898e1 xor      r8d, r8d
1804898e4 mov      rcx, rax
1804898e7 mov      rbx, rax
1804898ea call     0x181811ef0 ; 1075:System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray
1804898ef cmp      byte ptr [rip + 0x2eb3c38], 0
1804898f6 mov      rcx, qword ptr [rip + 0x2cf9a03]
1804898fd mov      rdx, qword ptr [rcx + 0xb8]
180489904 mov      qword ptr [rdx + 8], rbx
180489908 jne      0x180489971
18048990a lea      rcx, [rip + 0x2d19bbf]
180489911 call     0x180309af0 ; 
180489916 lea      rcx, [rip + 0x2d3fd53]
18048991d call     0x180309af0 ; 
180489922 lea      rcx, [rip + 0x2d2d6df]
180489929 call     0x180309af0 ; 
18048992e lea      rcx, [rip + 0x2cf417b]
180489935 call     0x180309af0 ; 
18048993a lea      rcx, [rip + 0x2cf59c7]
180489941 call     0x180309af0 ; 
180489946 lea      rcx, [rip + 0x2cf92a3]
18048994d call     0x180309af0 ; 
180489952 lea      rcx, [rip + 0x2cfbfaf]
180489959 call     0x180309af0 ; 
18048995e lea      rcx, [rip + 0x2d3da6b]
180489965 call     0x180309af0 ; 
18048996a mov      byte ptr [rip + 0x2eb3bbd], 1
180489971 mov      rcx, qword ptr [rip + 0x2d3da58]
180489978 cmp      dword ptr [rcx + 0xe4], 0
18048997f jne      0x180489986
180489981 call     0x180309de0 ; 
180489986 mov      rbx, qword ptr [rip + 0x2cf597b]
18048998d cmp      qword ptr [rbx + 0x38], 0
180489992 jne      0x18048999c
180489994 mov      rcx, rbx
180489997 call     0x18030dbb0 ; 
18048999c mov      rax, qword ptr [rbx + 0x38]
1804899a0 mov      rax, qword ptr [rax + 8]
1804899a4 test     byte ptr [rax + 0x135], 1
1804899ab jne      0x1804899b5
1804899ad mov      rcx, rax
1804899b0 call     0x18030db30 ; 
1804899b5 mov      rax, qword ptr [rax + 0xb8]
1804899bc cmp      byte ptr [rax], 0
1804899bf jne      0x180489a1f
1804899c1 mov      rcx, qword ptr [rip + 0x2d2d640]
1804899c8 call     0x180309ce0 ; 
1804899cd cmp      byte ptr [rip + 0x2eb3b5f], 0
1804899d4 mov      rbx, rax
1804899d7 jne      0x1804899ec
1804899d9 lea      rcx, [rip + 0x2ce20e8]
1804899e0 call     0x180309af0 ; 
1804899e5 mov      byte ptr [rip + 0x2eb3b47], 1
1804899ec mov      rdx, qword ptr [rip + 0x2ce20d5]
1804899f3 mov      rcx, rbx
1804899f6 call     0x180431940 ; 78:Mono.Globalization.Unicode.ContractionComparer..ctor | 80:.<>c..ctor | 89:Mono.Globalization.Unicode.SortKeyBuffer..ctor | 115:Mono.Math.Prime.Generator.PrimeGeneratorBase..ctor
1804899fb mov      rcx, qword ptr [rip + 0x2d3d9ce]
180489a02 cmp      dword ptr [rcx + 0xe4], 0
180489a09 jne      0x180489a10
180489a0b call     0x180309de0 ; 
180489a10 mov      rdx, qword ptr [rip + 0x2cfbef1]
180489a17 mov      rcx, rbx
180489a1a call     0x1808b6330 ; 
180489a1f mov      rcx, qword ptr [rip + 0x2d3d9aa]
180489a26 cmp      dword ptr [rcx + 0xe4], 0
180489a2d jne      0x180489a34
180489a2f call     0x180309de0 ; 
180489a34 mov      rbx, qword ptr [rip + 0x2cf4075]
180489a3b cmp      qword ptr [rbx + 0x38], 0
180489a40 jne      0x180489a4a
180489a42 mov      rcx, rbx
180489a45 call     0x18030dbb0 ; 
180489a4a mov      rax, qword ptr [rbx + 0x38]
180489a4e mov      rax, qword ptr [rax + 8]
180489a52 test     byte ptr [rax + 0x135], 1
180489a59 jne      0x180489a63
180489a5b mov      rcx, rax
180489a5e call     0x18030db30 ; 
180489a63 mov      rax, qword ptr [rax + 0xb8]
180489a6a cmp      byte ptr [rax], 0
180489a6d jne      0x180489ab6
180489a6f mov      rcx, qword ptr [rip + 0x2d3fbfa]
180489a76 call     0x180309ce0 ; 
180489a7b mov      rdx, qword ptr [rip + 0x2d19a4e]
180489a82 mov      rcx, rax
180489a85 mov      rbx, rax
180489a88 call     0x180f9e230 ; 
180489a8d mov      rcx, qword ptr [rip + 0x2d3d93c]
180489a94 cmp      dword ptr [rcx + 0xe4], 0
180489a9b jne      0x180489aa2
180489a9d call     0x180309de0 ; 
180489aa2 mov      rdx, qword ptr [rip + 0x2cf9147]
180489aa9 mov      rcx, rbx
180489aac add      rsp, 0x20
180489ab0 pop      rbx
180489ab1 jmp      0x1808b6330 ; 
180489ab6 add      rsp, 0x20
180489aba pop      rbx
180489abb ret      

FUNCTION 9530 BattleScore RegisterFormatter 0x1804895b0 464
1804895b0 push     rbx
1804895b2 sub      rsp, 0x20
1804895b6 cmp      byte ptr [rip + 0x2eb3f71], 0
1804895bd jne      0x180489626
1804895bf lea      rcx, [rip + 0x2d19f0a]
1804895c6 call     0x180309af0 ; 
1804895cb lea      rcx, [rip + 0x2d4009e]
1804895d2 call     0x180309af0 ; 
1804895d7 lea      rcx, [rip + 0x2d2da2a]
1804895de call     0x180309af0 ; 
1804895e3 lea      rcx, [rip + 0x2cf44c6]
1804895ea call     0x180309af0 ; 
1804895ef lea      rcx, [rip + 0x2cf5d12]
1804895f6 call     0x180309af0 ; 
1804895fb lea      rcx, [rip + 0x2cf95ee]
180489602 call     0x180309af0 ; 
180489607 lea      rcx, [rip + 0x2cfc2fa]
18048960e call     0x180309af0 ; 
180489613 lea      rcx, [rip + 0x2d3ddb6]
18048961a call     0x180309af0 ; 
18048961f mov      byte ptr [rip + 0x2eb3f08], 1
180489626 mov      rcx, qword ptr [rip + 0x2d3dda3]
18048962d cmp      dword ptr [rcx + 0xe4], 0
180489634 jne      0x18048963b
180489636 call     0x180309de0 ; 
18048963b mov      rbx, qword ptr [rip + 0x2cf5cc6]
180489642 cmp      qword ptr [rbx + 0x38], 0
180489647 jne      0x180489651
180489649 mov      rcx, rbx
18048964c call     0x18030dbb0 ; 
180489651 mov      rax, qword ptr [rbx + 0x38]
180489655 mov      rax, qword ptr [rax + 8]
180489659 test     byte ptr [rax + 0x135], 1
180489660 jne      0x18048966a
180489662 mov      rcx, rax
180489665 call     0x18030db30 ; 
18048966a mov      rax, qword ptr [rax + 0xb8]
180489671 cmp      byte ptr [rax], 0
180489674 jne      0x1804896d4
180489676 mov      rcx, qword ptr [rip + 0x2d2d98b]
18048967d call     0x180309ce0 ; 
180489682 cmp      byte ptr [rip + 0x2eb3eaa], 0
180489689 mov      rbx, rax
18048968c jne      0x1804896a1
18048968e lea      rcx, [rip + 0x2ce2433]
180489695 call     0x180309af0 ; 
18048969a mov      byte ptr [rip + 0x2eb3e92], 1
1804896a1 mov      rdx, qword ptr [rip + 0x2ce2420]
1804896a8 mov      rcx, rbx
1804896ab call     0x180431940 ; 78:Mono.Globalization.Unicode.ContractionComparer..ctor | 80:.<>c..ctor | 89:Mono.Globalization.Unicode.SortKeyBuffer..ctor | 115:Mono.Math.Prime.Generator.PrimeGeneratorBase..ctor
1804896b0 mov      rcx, qword ptr [rip + 0x2d3dd19]
1804896b7 cmp      dword ptr [rcx + 0xe4], 0
1804896be jne      0x1804896c5
1804896c0 call     0x180309de0 ; 
1804896c5 mov      rdx, qword ptr [rip + 0x2cfc23c]
1804896cc mov      rcx, rbx
1804896cf call     0x1808b6330 ; 
1804896d4 mov      rcx, qword ptr [rip + 0x2d3dcf5]
1804896db cmp      dword ptr [rcx + 0xe4], 0
1804896e2 jne      0x1804896e9
1804896e4 call     0x180309de0 ; 
1804896e9 mov      rbx, qword ptr [rip + 0x2cf43c0]
1804896f0 cmp      qword ptr [rbx + 0x38], 0
1804896f5 jne      0x1804896ff
1804896f7 mov      rcx, rbx
1804896fa call     0x18030dbb0 ; 
1804896ff mov      rax, qword ptr [rbx + 0x38]
180489703 mov      rax, qword ptr [rax + 8]
180489707 test     byte ptr [rax + 0x135], 1
18048970e jne      0x180489718
180489710 mov      rcx, rax
180489713 call     0x18030db30 ; 
180489718 mov      rax, qword ptr [rax + 0xb8]
18048971f cmp      byte ptr [rax], 0
180489722 jne      0x18048976b
180489724 mov      rcx, qword ptr [rip + 0x2d3ff45]
18048972b call     0x180309ce0 ; 
180489730 mov      rdx, qword ptr [rip + 0x2d19d99]
180489737 mov      rcx, rax
18048973a mov      rbx, rax
18048973d call     0x180f9e230 ; 
180489742 mov      rcx, qword ptr [rip + 0x2d3dc87]
180489749 cmp      dword ptr [rcx + 0xe4], 0
180489750 jne      0x180489757
180489752 call     0x180309de0 ; 
180489757 mov      rdx, qword ptr [rip + 0x2cf9492]
18048975e mov      rcx, rbx
180489761 add      rsp, 0x20
180489765 pop      rbx
180489766 jmp      0x1808b6330 ; 
18048976b add      rsp, 0x20
18048976f pop      rbx
180489770 ret      

FUNCTION 9530 BattleScore Serialize 0x180489780 192
180489780 mov      qword ptr [rsp + 8], rbx
180489785 push     rdi
180489786 sub      rsp, 0x30
18048978a cmp      byte ptr [rip + 0x2eb3d9e], 0
180489791 mov      rdi, rdx
180489794 mov      rbx, rcx
180489797 jne      0x1804897ac
180489799 lea      rcx, [rip + 0x2d05c58]
1804897a0 call     0x180309af0 ; 
1804897a5 mov      byte ptr [rip + 0x2eb3d83], 1
1804897ac cmp      byte ptr [rip + 0x2eb3e40], 0
1804897b3 jne      0x1804897c8
1804897b5 lea      rcx, [rip + 0x2cf32ec]
1804897bc call     0x180309af0 ; 
1804897c1 mov      byte ptr [rip + 0x2eb3e2b], 1
1804897c8 cmp      dword ptr [rbx + 0x18], 0x38
1804897cc jge      0x1804897de
1804897ce xor      r8d, r8d
1804897d1 mov      edx, 0x38
1804897d6 mov      rcx, rbx
1804897d9 call     0x1817077b0 ; 11113:MemoryPack.MemoryPackWriter.RequestNewBuffer
1804897de movups   xmm0, xmmword ptr [rbx + 8]
1804897e2 mov      rdx, qword ptr [rip + 0x2cf32bf]
1804897e9 lea      rcx, [rsp + 0x20]
1804897ee movaps   xmmword ptr [rsp + 0x20], xmm0
1804897f3 call     0x1804192f0 ; 201:System.DateTime.System.IConvertible.ToDateTime | 257:System.Int64.System.IConvertible.ToInt64 | 315:System.TimeSpan.get_Ticks | 334:System.UInt64.System.IConvertible.ToUInt64
1804897f8 movups   xmm0, xmmword ptr [rdi]
1804897fb xor      r8d, r8d
1804897fe mov      edx, 0x38
180489803 movups   xmm1, xmmword ptr [rdi + 0x10]
180489807 mov      rcx, rbx
18048980a movups   xmm2, xmmword ptr [rdi + 0x20]
18048980e movsd    xmm3, qword ptr [rdi + 0x30]
180489813 movups   xmmword ptr [rax], xmm0
180489816 movups   xmmword ptr [rax + 0x10], xmm1
18048981a movups   xmmword ptr [rax + 0x20], xmm2
18048981e movsd    qword ptr [rax + 0x30], xmm3
180489823 mov      rbx, qword ptr [rsp + 0x40]
180489828 add      rsp, 0x30
18048982c pop      rdi
18048982d jmp      0x180487e40 ; 

FUNCTION 9530 BattleScore Deserialize 0x1804892a0 192
1804892a0 mov      qword ptr [rsp + 8], rbx
1804892a5 push     rdi
1804892a6 sub      rsp, 0x30
1804892aa cmp      byte ptr [rip + 0x2eb427f], 0
1804892b1 mov      rdi, rdx
1804892b4 mov      rbx, rcx
1804892b7 jne      0x1804892cc
1804892b9 lea      rcx, [rip + 0x2d01d98]
1804892c0 call     0x180309af0 ; 
1804892c5 mov      byte ptr [rip + 0x2eb4264], 1
1804892cc cmp      byte ptr [rip + 0x2eb4324], 0
1804892d3 jne      0x1804892e8
1804892d5 lea      rcx, [rip + 0x2cf3734]
1804892dc call     0x180309af0 ; 
1804892e1 mov      byte ptr [rip + 0x2eb430f], 1
1804892e8 cmp      dword ptr [rbx + 0x30], 0x38
1804892ec jge      0x180489300
1804892ee xor      r8d, r8d
1804892f1 mov      edx, 0x38
1804892f6 mov      rcx, rbx
1804892f9 call     0x181705190 ; 11110:MemoryPack.MemoryPackReader.GetNextSpan
1804892fe jmp      0x18048931a ; 
180489300 movups   xmm0, xmmword ptr [rbx + 0x20]
180489304 mov      rdx, qword ptr [rip + 0x2cf3705]
18048930b lea      rcx, [rsp + 0x20]
180489310 movaps   xmmword ptr [rsp + 0x20], xmm0
180489315 call     0x1804192f0 ; 201:System.DateTime.System.IConvertible.ToDateTime | 257:System.Int64.System.IConvertible.ToInt64 | 315:System.TimeSpan.get_Ticks | 334:System.UInt64.System.IConvertible.ToUInt64
18048931a movups   xmm0, xmmword ptr [rax]
18048931d xor      r8d, r8d
180489320 mov      edx, 0x38
180489325 movups   xmm1, xmmword ptr [rax + 0x10]
180489329 mov      rcx, rbx
18048932c movups   xmm2, xmmword ptr [rax + 0x20]
180489330 movsd    xmm3, qword ptr [rax + 0x30]
180489335 movups   xmmword ptr [rdi], xmm0
180489338 movups   xmmword ptr [rdi + 0x10], xmm1
18048933c movups   xmmword ptr [rdi + 0x20], xmm2
180489340 movsd    qword ptr [rdi + 0x30], xmm3
180489345 mov      rbx, qword ptr [rsp + 0x40]
18048934a add      rsp, 0x30
18048934e pop      rdi
18048934f jmp      0x180486dd0 ; 

FUNCTION 9543 CardInstance GetGlobalTempCardId 0x18048a880 112
18048a880 sub      rsp, 0x28
18048a884 cmp      byte ptr [rip + 0x2eb2cb4], 0
18048a88b jne      0x18048a8a0
18048a88d lea      rcx, [rip + 0x2cfed44]
18048a894 call     0x180309af0 ; 
18048a899 mov      byte ptr [rip + 0x2eb2c9f], 1
18048a8a0 mov      r8, qword ptr [rip + 0x2cfed31]
18048a8a7 cmp      dword ptr [r8 + 0xe4], 0
18048a8af jne      0x18048a8c0
18048a8b1 mov      rcx, r8
18048a8b4 call     0x180309de0 ; 
18048a8b9 mov      r8, qword ptr [rip + 0x2cfed18]
18048a8c0 mov      rax, qword ptr [r8 + 0xb8]
18048a8c7 mov      ecx, dword ptr [rax]
18048a8c9 inc      ecx
18048a8cb movzx    edx, cx
18048a8ce mov      dword ptr [rax], edx
18048a8d0 mov      rax, qword ptr [rip + 0x2cfed01]
18048a8d7 mov      rcx, qword ptr [rax + 0xb8]
18048a8de mov      eax, dword ptr [rcx]
18048a8e0 add      eax, 0x7ffefff6
18048a8e5 add      rsp, 0x28
18048a8e9 ret      

FUNCTION 9543 CardInstance HasAdvantage 0x18048aa20 160
18048aa20 mov      qword ptr [rsp + 8], rbx
18048aa25 push     rdi
18048aa26 sub      rsp, 0x20
18048aa2a cmp      byte ptr [rip + 0x2eb2b0f], 0
18048aa31 mov      ebx, edx
18048aa33 mov      edi, ecx
18048aa35 jne      0x18048aa4a
18048aa37 lea      rcx, [rip + 0x2cfeb9a]
18048aa3e call     0x180309af0 ; 
18048aa43 mov      byte ptr [rip + 0x2eb2af6], 1
18048aa4a mov      rax, qword ptr [rip + 0x2cfeb87]
18048aa51 cmp      dword ptr [rax + 0xe4], 0
18048aa58 jne      0x18048aa69
18048aa5a mov      rcx, rax
18048aa5d call     0x180309de0 ; 
18048aa62 mov      rax, qword ptr [rip + 0x2cfeb6f]
18048aa69 mov      rax, qword ptr [rax + 0xb8]
18048aa70 mov      rcx, qword ptr [rax + 0x1c8]
18048aa77 test     rcx, rcx
18048aa7a je       0x18048aaa6
18048aa7c lea      eax, [rbx + rbx*2]
18048aa7f lea      edx, [rdi + rax*2]
18048aa82 cmp      edx, dword ptr [rcx + 0x18]
18048aa85 jae      0x18048aaac
18048aa87 mov      rbx, qword ptr [rsp + 0x30]
18048aa8c movsxd   rax, edx
18048aa8f movsd    xmm0, qword ptr [rcx + rax*8 + 0x20]
18048aa95 comisd   xmm0, xmmword ptr [rip + 0x217daab]
18048aa9d seta     al
18048aaa0 add      rsp, 0x20
18048aaa4 pop      rdi
18048aaa5 ret      
18048aaa6 call     0x180309d40 ; 
18048aaac call     0x180309d30 ; 

FUNCTION 9543 CardInstance GetMultiplier 0x18048a8f0 144
18048a8f0 mov      qword ptr [rsp + 8], rbx
18048a8f5 push     rdi
18048a8f6 sub      rsp, 0x20
18048a8fa cmp      byte ptr [rip + 0x2eb2c40], 0
18048a901 mov      ebx, edx
18048a903 mov      edi, ecx
18048a905 jne      0x18048a91a
18048a907 lea      rcx, [rip + 0x2cfecca]
18048a90e call     0x180309af0 ; 
18048a913 mov      byte ptr [rip + 0x2eb2c27], 1
18048a91a mov      rax, qword ptr [rip + 0x2cfecb7]
18048a921 cmp      dword ptr [rax + 0xe4], 0
18048a928 jne      0x18048a939
18048a92a mov      rcx, rax
18048a92d call     0x180309de0 ; 
18048a932 mov      rax, qword ptr [rip + 0x2cfec9f]
18048a939 mov      rax, qword ptr [rax + 0xb8]
18048a940 mov      rcx, qword ptr [rax + 0x1c8]
18048a947 test     rcx, rcx
18048a94a je       0x18048a96b
18048a94c lea      eax, [rbx + rbx*2]
18048a94f lea      edx, [rdi + rax*2]
18048a952 cmp      edx, dword ptr [rcx + 0x18]
18048a955 jae      0x18048a971
18048a957 mov      rbx, qword ptr [rsp + 0x30]
18048a95c movsxd   rax, edx
18048a95f movsd    xmm0, qword ptr [rcx + rax*8 + 0x20]
18048a965 add      rsp, 0x20
18048a969 pop      rdi
18048a96a ret      
18048a96b call     0x180309d40 ; 
18048a971 call     0x180309d30 ; 

FUNCTION 9543 CardInstance IsValid 0x18048aac0 32
18048aac0 cmp      dword ptr [rcx + 4], 0
18048aac4 jg       0x18048aac9
18048aac6 xor      al, al
18048aac8 ret      
18048aac9 cmp      dword ptr [rcx + 0xc], 0
18048aacd seta     al
18048aad0 ret      

FUNCTION 9543 CardInstance GetTraitSlotTypesUnsafe 0x18048a9d0 80
18048a9d0 mov      qword ptr [rsp + 8], rbx
18048a9d5 push     rdi
18048a9d6 sub      rsp, 0x20
18048a9da cmp      byte ptr [rip + 0x2eb2b61], 0
18048a9e1 mov      rdi, rdx
18048a9e4 mov      rbx, rcx
18048a9e7 jne      0x18048a9fc
18048a9e9 lea      rcx, [rip + 0x2d314b8]
18048a9f0 call     0x180309af0 ; 
18048a9f5 mov      byte ptr [rip + 0x2eb2b46], 1
18048a9fc mov      qword ptr [rbx + 8], 3
18048aa04 lea      rax, [rdi + 0x28]
18048aa08 mov      qword ptr [rbx], rax
18048aa0b mov      rax, rbx
18048aa0e mov      rbx, qword ptr [rsp + 0x30]
18048aa13 add      rsp, 0x20
18048aa17 pop      rdi
18048aa18 ret      

FUNCTION 9543 CardInstance GetTraitIdsUnsafe 0x18048a980 80
18048a980 mov      qword ptr [rsp + 8], rbx
18048a985 push     rdi
18048a986 sub      rsp, 0x20
18048a98a cmp      byte ptr [rip + 0x2eb2bb2], 0
18048a991 mov      rdi, rdx
18048a994 mov      rbx, rcx
18048a997 jne      0x18048a9ac
18048a999 lea      rcx, [rip + 0x2d2c280]
18048a9a0 call     0x180309af0 ; 
18048a9a5 mov      byte ptr [rip + 0x2eb2b97], 1
18048a9ac mov      qword ptr [rbx + 8], 3
18048a9b4 lea      rax, [rdi + 0x2e]
18048a9b8 mov      qword ptr [rbx], rax
18048a9bb mov      rax, rbx
18048a9be mov      rbx, qword ptr [rsp + 0x30]
18048a9c3 add      rsp, 0x20
18048a9c7 pop      rdi
18048a9c8 ret      

FUNCTION 9543 CardInstance .cctor 0x18048ae20 704
18048ae20 mov      qword ptr [rsp + 8], rbx
18048ae25 push     rbp
18048ae26 mov      rbp, rsp
18048ae29 sub      rsp, 0x60
18048ae2d cmp      byte ptr [rip + 0x2eb2710], 0
18048ae34 jne      0x18048ae61
18048ae36 lea      rcx, [rip + 0x2cfe79b]
18048ae3d call     0x180309af0 ; 
18048ae42 lea      rcx, [rip + 0x2ce0dd7]
18048ae49 call     0x180309af0 ; 
18048ae4e lea      rcx, [rip + 0x2d149fb]
18048ae55 call     0x180309af0 ; 
18048ae5a mov      byte ptr [rip + 0x2eb26e3], 1
18048ae61 mov      rax, qword ptr [rip + 0x2cfe770]
18048ae68 xor      r9d, r9d
18048ae6b movdqa   xmm2, xmmword ptr [rip + 0x217d70d]
18048ae73 mov      edx, 1
18048ae78 movdqa   xmm3, xmmword ptr [rip + 0x217d710]
18048ae80 mov      r8d, 2
18048ae86 mov      qword ptr [rbp - 0x14], 0xffffffffffff0000
18048ae8e mov      rcx, qword ptr [rax + 0xb8]
18048ae95 mov      eax, 0xffffffff
18048ae9a mov      dword ptr [rbp - 0x40], eax
18048ae9d mov      qword ptr [rbp - 0x3c], r9
18048aea1 mov      dword ptr [rbp - 0x34], r9d
18048aea5 movaps   xmm0, xmmword ptr [rbp - 0x40]
18048aea9 mov      dword ptr [rcx], edx
18048aeab mov      rax, qword ptr [rip + 0x2cfe726]
18048aeb2 mov      qword ptr [rbp - 0x3c], r9
18048aeb6 mov      qword ptr [rbp - 0x20], r9
18048aeba mov      dword ptr [rbp - 0x18], r9d
18048aebe mov      rcx, qword ptr [rax + 0xb8]
18048aec5 mov      qword ptr [rbp - 0xc], r9
18048aec9 mov      dword ptr [rbp - 4], r9d
18048aecd movaps   xmm1, xmmword ptr [rbp - 0x10]
18048aed1 movups   xmmword ptr [rcx + 4], xmm0
18048aed5 mov      qword ptr [rbp - 0xc], r9
18048aed9 movaps   xmm0, xmmword ptr [rbp - 0x20]
18048aedd movups   xmmword ptr [rcx + 0x14], xmm2
18048aee1 mov      qword ptr [rbp - 0x20], r9
18048aee5 movups   xmmword ptr [rcx + 0x24], xmm0
18048aee9 mov      dword ptr [rbp - 0x18], r9d
18048aeed movaps   xmm0, xmmword ptr [rbp - 0x40]
18048aef1 movups   xmmword ptr [rcx + 0x34], xmm1
18048aef5 mov      rax, qword ptr [rip + 0x2cfe6dc]
18048aefc mov      qword ptr [rbp - 0x3c], rdx
18048af00 mov      dword ptr [rbp - 0x40], edx
18048af03 mov      dword ptr [rbp - 0x34], edx
18048af06 mov      edx, 3
18048af0b mov      rcx, qword ptr [rax + 0xb8]
18048af12 mov      word ptr [rbp - 0x14], r9w
18048af17 mov      dword ptr [rbp - 4], r9d
18048af1b movaps   xmm1, xmmword ptr [rbp - 0x10]
18048af1f movups   xmmword ptr [rcx + 0x44], xmm0
18048af23 mov      qword ptr [rbp - 0xc], r9
18048af27 movaps   xmm0, xmmword ptr [rbp - 0x20]
18048af2b movups   xmmword ptr [rcx + 0x54], xmm2
18048af2f mov      qword ptr [rbp - 0x20], r9
18048af33 movups   xmmword ptr [rcx + 0x64], xmm0
18048af37 mov      dword ptr [rbp - 0x18], r9d
18048af3b movaps   xmm0, xmmword ptr [rbp - 0x40]
18048af3f xorps    xmm2, xmm2
18048af42 movups   xmmword ptr [rcx + 0x74], xmm1
18048af46 mov      rax, qword ptr [rip + 0x2cfe68b]
18048af4d mov      qword ptr [rbp - 0x3c], r8
18048af51 mov      dword ptr [rbp - 0x40], r8d
18048af55 mov      dword ptr [rbp - 0x34], 4
18048af5c mov      rcx, qword ptr [rax + 0xb8]
18048af63 mov      dword ptr [rbp - 0x14], 0x10000
18048af6a mov      dword ptr [rbp - 4], r9d
18048af6e mov      dword ptr [rbp - 0x10], 0x30002
18048af75 movaps   xmm1, xmmword ptr [rbp - 0x10]
18048af79 movups   xmmword ptr [rcx + 0x84], xmm0
18048af80 movaps   xmm0, xmmword ptr [rbp - 0x20]
18048af84 movups   xmmword ptr [rcx + 0x94], xmm3
18048af8b movups   xmmword ptr [rcx + 0xa4], xmm0
18048af92 movaps   xmm0, xmmword ptr [rbp - 0x40]
18048af96 movups   xmmword ptr [rcx + 0xb4], xmm1
18048af9d mov      rax, qword ptr [rip + 0x2cfe634]
18048afa4 xorps    xmm1, xmm1
18048afa7 mov      qword ptr [rbp - 0x3c], rdx
18048afab mov      dword ptr [rbp - 0x40], edx
18048afae mov      dword ptr [rbp - 0x34], edx
18048afb1 mov      rcx, qword ptr [rax + 0xb8]
18048afb8 movups   xmmword ptr [rcx + 0xc4], xmm0
18048afbf movaps   xmm0, xmmword ptr [rbp - 0x40]
18048afc3 movups   xmmword ptr [rcx + 0xd4], xmm3
18048afca movups   xmmword ptr [rcx + 0xe4], xmm2
18048afd1 movups   xmmword ptr [rcx + 0xf4], xmm1
18048afd8 mov      rax, qword ptr [rip + 0x2cfe5f9]
18048afdf mov      rcx, qword ptr [rax + 0xb8]
18048afe6 movups   xmmword ptr [rcx + 0x104], xmm0
18048afed movups   xmmword ptr [rcx + 0x114], xmm3
18048aff4 movups   xmmword ptr [rcx + 0x124], xmm1
18048affb movups   xmmword ptr [rcx + 0x134], xmm2
18048b002 mov      rax, qword ptr [rip + 0x2cfe5cf]
18048b009 mov      edx, 0x24
18048b00e mov      dword ptr [rbp - 0x34], r8d
18048b012 mov      qword ptr [rbp - 0x3c], 4
18048b01a mov      dword ptr [rbp - 0x40], 4
18048b021 mov      rcx, qword ptr [rax + 0xb8]
18048b028 movaps   xmm0, xmmword ptr [rbp - 0x40]
18048b02c mov      qword ptr [rbp - 0x3c], 5
18048b034 mov      dword ptr [rbp - 0x40], 5
18048b03b movups   xmmword ptr [rcx + 0x144], xmm0
18048b042 mov      dword ptr [rbp - 0x34], 5
18048b049 movaps   xmm0, xmmword ptr [rbp - 0x40]
18048b04d movups   xmmword ptr [rcx + 0x154], xmm3
18048b054 movups   xmmword ptr [rcx + 0x164], xmm1
18048b05b movups   xmmword ptr [rcx + 0x174], xmm2
18048b062 mov      rax, qword ptr [rip + 0x2cfe56f]
18048b069 mov      rcx, qword ptr [rax + 0xb8]
18048b070 movups   xmmword ptr [rcx + 0x184], xmm0
18048b077 movups   xmmword ptr [rcx + 0x194], xmm3
18048b07e movups   xmmword ptr [rcx + 0x1a4], xmm1
18048b085 movups   xmmword ptr [rcx + 0x1b4], xmm2
18048b08c mov      rcx, qword ptr [rip + 0x2ce0b8d]
18048b093 call     0x180308ef0 ; 
18048b098 mov      rdx, qword ptr [rip + 0x2d147b1]
18048b09f xor      r8d, r8d
18048b0a2 mov      rcx, rax
18048b0a5 mov      rbx, rax
18048b0a8 call     0x181811ef0 ; 1075:System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray
18048b0ad mov      rcx, qword ptr [rip + 0x2cfe524]
18048b0b4 mov      rdx, qword ptr [rcx + 0xb8]
18048b0bb xor      ecx, ecx
18048b0bd mov      qword ptr [rdx + 0x1c8], rbx
18048b0c4 mov      rbx, qword ptr [rsp + 0x70]
18048b0c9 add      rsp, 0x60
18048b0cd pop      rbp
18048b0ce jmp      0x18048aae0 ; 9543:ifapp.Game.Data.CardInstance.RegisterFormatter

FUNCTION 9543 CardInstance RegisterFormatter 0x18048aae0 656
18048aae0 push     rbx
18048aae2 sub      rsp, 0x20
18048aae6 cmp      byte ptr [rip + 0x2eb2a58], 0
18048aaed jne      0x18048ab8a
18048aaf3 lea      rcx, [rip + 0x2d18bb6]
18048aafa call     0x180309af0 ; 
18048aaff lea      rcx, [rip + 0x2d3ed32]
18048ab06 call     0x180309af0 ; 
18048ab0b lea      rcx, [rip + 0x2d2ee86]
18048ab12 call     0x180309af0 ; 
18048ab17 lea      rcx, [rip + 0x2cf4882]
18048ab1e call     0x180309af0 ; 
18048ab23 lea      rcx, [rip + 0x2cf301e]
18048ab2a call     0x180309af0 ; 
18048ab2f lea      rcx, [rip + 0x2cf4902]
18048ab36 call     0x180309af0 ; 
18048ab3b lea      rcx, [rip + 0x2cfb0ae]
18048ab42 call     0x180309af0 ; 
18048ab47 lea      rcx, [rip + 0x2cf838a]
18048ab4e call     0x180309af0 ; 
18048ab53 lea      rcx, [rip + 0x2cfb126]
18048ab5a call     0x180309af0 ; 
18048ab5f lea      rcx, [rip + 0x2d3c86a]
18048ab66 call     0x180309af0 ; 
18048ab6b lea      rcx, [rip + 0x2ce6b96]
18048ab72 call     0x180309af0 ; 
18048ab77 lea      rcx, [rip + 0x2d3b3f2]
18048ab7e call     0x180309af0 ; 
18048ab83 mov      byte ptr [rip + 0x2eb29bb], 1
18048ab8a mov      rcx, qword ptr [rip + 0x2d3c83f]
18048ab91 cmp      dword ptr [rcx + 0xe4], 0
18048ab98 jne      0x18048ab9f
18048ab9a call     0x180309de0 ; 
18048ab9f mov      rbx, qword ptr [rip + 0x2cf4892]
18048aba6 cmp      qword ptr [rbx + 0x38], 0
18048abab jne      0x18048abb5
18048abad mov      rcx, rbx
18048abb0 call     0x18030dbb0 ; 
18048abb5 mov      rax, qword ptr [rbx + 0x38]
18048abb9 mov      rax, qword ptr [rax + 8]
18048abbd test     byte ptr [rax + 0x135], 1
18048abc4 jne      0x18048abce
18048abc6 mov      rcx, rax
18048abc9 call     0x18030db30 ; 
18048abce mov      rax, qword ptr [rax + 0xb8]
18048abd5 cmp      byte ptr [rax], 0
18048abd8 jne      0x18048ac38
18048abda mov      rcx, qword ptr [rip + 0x2d2edb7]
18048abe1 call     0x180309ce0 ; 
18048abe6 cmp      byte ptr [rip + 0x2eb295d], 0
18048abed mov      rbx, rax
18048abf0 jne      0x18048ac05
18048abf2 lea      rcx, [rip + 0x2ce1097]
18048abf9 call     0x180309af0 ; 
18048abfe mov      byte ptr [rip + 0x2eb2945], 1
18048ac05 mov      rdx, qword ptr [rip + 0x2ce1084]
18048ac0c mov      rcx, rbx
18048ac0f call     0x180431940 ; 78:Mono.Globalization.Unicode.ContractionComparer..ctor | 80:.<>c..ctor | 89:Mono.Globalization.Unicode.SortKeyBuffer..ctor | 115:Mono.Math.Prime.Generator.PrimeGeneratorBase..ctor
18048ac14 mov      rcx, qword ptr [rip + 0x2d3c7b5]
18048ac1b cmp      dword ptr [rcx + 0xe4], 0
18048ac22 jne      0x18048ac29
18048ac24 call     0x180309de0 ; 
18048ac29 mov      rdx, qword ptr [rip + 0x2cfb050]
18048ac30 mov      rcx, rbx
18048ac33 call     0x1808b6330 ; 
18048ac38 mov      rcx, qword ptr [rip + 0x2d3c791]
18048ac3f cmp      dword ptr [rcx + 0xe4], 0
18048ac46 jne      0x18048ac4d
18048ac48 call     0x180309de0 ; 
18048ac4d mov      rbx, qword ptr [rip + 0x2cf2ef4]
18048ac54 cmp      qword ptr [rbx + 0x38], 0
18048ac59 jne      0x18048ac63
18048ac5b mov      rcx, rbx
18048ac5e call     0x18030dbb0 ; 
18048ac63 mov      rax, qword ptr [rbx + 0x38]
18048ac67 mov      rax, qword ptr [rax + 8]
18048ac6b test     byte ptr [rax + 0x135], 1
18048ac72 jne      0x18048ac7c
18048ac74 mov      rcx, rax
18048ac77 call     0x18030db30 ; 
18048ac7c mov      rax, qword ptr [rax + 0xb8]
18048ac83 cmp      byte ptr [rax], 0
18048ac86 jne      0x18048acca
18048ac88 mov      rcx, qword ptr [rip + 0x2d3eba9]
18048ac8f call     0x180309ce0 ; 
18048ac94 mov      rdx, qword ptr [rip + 0x2d18a15]
18048ac9b mov      rcx, rax
18048ac9e mov      rbx, rax
18048aca1 call     0x180f9e230 ; 
18048aca6 mov      rcx, qword ptr [rip + 0x2d3c723]
18048acad cmp      dword ptr [rcx + 0xe4], 0
18048acb4 jne      0x18048acbb
18048acb6 call     0x180309de0 ; 
18048acbb mov      rdx, qword ptr [rip + 0x2cf8216]
18048acc2 mov      rcx, rbx
18048acc5 call     0x1808b6330 ; 
18048acca mov      rcx, qword ptr [rip + 0x2d3c6ff]
18048acd1 cmp      dword ptr [rcx + 0xe4], 0
18048acd8 jne      0x18048acdf
18048acda call     0x180309de0 ; 
18048acdf mov      rbx, qword ptr [rip + 0x2cf46ba]
18048ace6 cmp      qword ptr [rbx + 0x38], 0
18048aceb jne      0x18048acf5
18048aced mov      rcx, rbx
18048acf0 call     0x18030dbb0 ; 
18048acf5 mov      rax, qword ptr [rbx + 0x38]
18048acf9 mov      rax, qword ptr [rax + 8]
18048acfd test     byte ptr [rax + 0x135], 1
18048ad04 jne      0x18048ad0e
18048ad06 mov      rcx, rax
18048ad09 call     0x18030db30 ; 
18048ad0e mov      rax, qword ptr [rax + 0xb8]
18048ad15 cmp      byte ptr [rax], 0
18048ad18 jne      0x18048ad61
18048ad1a mov      rcx, qword ptr [rip + 0x2d3b24f]
18048ad21 call     0x180309ce0 ; 
18048ad26 mov      rdx, qword ptr [rip + 0x2ce69db]
18048ad2d mov      rcx, rax
18048ad30 mov      rbx, rax
18048ad33 call     0x180c544b0 ; 
18048ad38 mov      rcx, qword ptr [rip + 0x2d3c691]
18048ad3f cmp      dword ptr [rcx + 0xe4], 0
18048ad46 jne      0x18048ad4d
18048ad48 call     0x180309de0 ; 
18048ad4d mov      rdx, qword ptr [rip + 0x2cfae9c]
18048ad54 mov      rcx, rbx
18048ad57 add      rsp, 0x20
18048ad5b pop      rbx
18048ad5c jmp      0x1808b6330 ; 
18048ad61 add      rsp, 0x20
18048ad65 pop      rbx
18048ad66 ret      

FUNCTION 9543 CardInstance Serialize 0x18048ad70 176
18048ad70 mov      qword ptr [rsp + 8], rbx
18048ad75 push     rdi
18048ad76 sub      rsp, 0x30
18048ad7a cmp      byte ptr [rip + 0x2eb27c5], 0
18048ad81 mov      rdi, rdx
18048ad84 mov      rbx, rcx
18048ad87 jne      0x18048ad9c
18048ad89 lea      rcx, [rip + 0x2d046f8]
18048ad90 call     0x180309af0 ; 
18048ad95 mov      byte ptr [rip + 0x2eb27aa], 1
18048ad9c cmp      byte ptr [rip + 0x2eb2850], 0
18048ada3 jne      0x18048adb8
18048ada5 lea      rcx, [rip + 0x2cf1cfc]
18048adac call     0x180309af0 ; 
18048adb1 mov      byte ptr [rip + 0x2eb283b], 1
18048adb8 cmp      dword ptr [rbx + 0x18], 0x40
18048adbc jge      0x18048adce
18048adbe xor      r8d, r8d
18048adc1 mov      edx, 0x40
18048adc6 mov      rcx, rbx
18048adc9 call     0x1817077b0 ; 11113:MemoryPack.MemoryPackWriter.RequestNewBuffer
18048adce movups   xmm0, xmmword ptr [rbx + 8]
18048add2 mov      rdx, qword ptr [rip + 0x2cf1ccf]
18048add9 lea      rcx, [rsp + 0x20]
18048adde movaps   xmmword ptr [rsp + 0x20], xmm0
18048ade3 call     0x1804192f0 ; 201:System.DateTime.System.IConvertible.ToDateTime | 257:System.Int64.System.IConvertible.ToInt64 | 315:System.TimeSpan.get_Ticks | 334:System.UInt64.System.IConvertible.ToUInt64
18048ade8 movups   xmm0, xmmword ptr [rdi]
18048adeb xor      r8d, r8d
18048adee mov      edx, 0x40
18048adf3 movups   xmm1, xmmword ptr [rdi + 0x10]
18048adf7 mov      rcx, rbx
18048adfa movups   xmm2, xmmword ptr [rdi + 0x20]
18048adfe movups   xmm3, xmmword ptr [rdi + 0x30]
18048ae02 movups   xmmword ptr [rax], xmm0
18048ae05 movups   xmmword ptr [rax + 0x10], xmm1
18048ae09 movups   xmmword ptr [rax + 0x20], xmm2
18048ae0d movups   xmmword ptr [rax + 0x30], xmm3
18048ae11 mov      rbx, qword ptr [rsp + 0x40]
18048ae16 add      rsp, 0x30
18048ae1a pop      rdi
18048ae1b jmp      0x180487e40 ; 

FUNCTION 9543 CardInstance Deserialize 0x18048a7c0 192
18048a7c0 mov      qword ptr [rsp + 8], rbx
18048a7c5 push     rdi
18048a7c6 sub      rsp, 0x30
18048a7ca cmp      byte ptr [rip + 0x2eb2d76], 0
18048a7d1 mov      rdi, rdx
18048a7d4 mov      rbx, rcx
18048a7d7 jne      0x18048a7ec
18048a7d9 lea      rcx, [rip + 0x2d00a30]
18048a7e0 call     0x180309af0 ; 
18048a7e5 mov      byte ptr [rip + 0x2eb2d5b], 1
18048a7ec cmp      byte ptr [rip + 0x2eb2e04], 0
18048a7f3 jne      0x18048a808
18048a7f5 lea      rcx, [rip + 0x2cf2214]
18048a7fc call     0x180309af0 ; 
18048a801 mov      byte ptr [rip + 0x2eb2def], 1
18048a808 cmp      dword ptr [rbx + 0x30], 0x40
18048a80c jge      0x18048a820
18048a80e xor      r8d, r8d
18048a811 mov      edx, 0x40
18048a816 mov      rcx, rbx
18048a819 call     0x181705190 ; 11110:MemoryPack.MemoryPackReader.GetNextSpan
18048a81e jmp      0x18048a83a ; 
18048a820 movups   xmm0, xmmword ptr [rbx + 0x20]
18048a824 mov      rdx, qword ptr [rip + 0x2cf21e5]
18048a82b lea      rcx, [rsp + 0x20]
18048a830 movaps   xmmword ptr [rsp + 0x20], xmm0
18048a835 call     0x1804192f0 ; 201:System.DateTime.System.IConvertible.ToDateTime | 257:System.Int64.System.IConvertible.ToInt64 | 315:System.TimeSpan.get_Ticks | 334:System.UInt64.System.IConvertible.ToUInt64
18048a83a movups   xmm0, xmmword ptr [rax]
18048a83d xor      r8d, r8d
18048a840 mov      edx, 0x40
18048a845 movups   xmm1, xmmword ptr [rax + 0x10]
18048a849 mov      rcx, rbx
18048a84c movups   xmm2, xmmword ptr [rax + 0x20]
18048a850 movups   xmm3, xmmword ptr [rax + 0x30]
18048a854 movups   xmmword ptr [rdi], xmm0
18048a857 movups   xmmword ptr [rdi + 0x10], xmm1
18048a85b movups   xmmword ptr [rdi + 0x20], xmm2
18048a85f movups   xmmword ptr [rdi + 0x30], xmm3
18048a863 mov      rbx, qword ptr [rsp + 0x40]
18048a868 add      rsp, 0x30
18048a86c pop      rdi
18048a86d jmp      0x180486dd0 ; 

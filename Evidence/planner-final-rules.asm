
FUNCTION 3946 CraftingConfirmationLayer _oY 0x1805e9ef0 416
1805e9ef0 mov      qword ptr [rsp + 8], rbx
1805e9ef5 mov      qword ptr [rsp + 0x10], rsi
1805e9efa push     rdi
1805e9efb sub      rsp, 0x50
1805e9eff cmp      byte ptr [rip + 0x2d53e61], 0
1805e9f06 mov      rbx, rdx
1805e9f09 mov      rsi, rcx
1805e9f0c jne      0x1805e9f69
1805e9f0e lea      rcx, [rip + 0x2bb57f3]
1805e9f15 call     0x180309af0 ; 
1805e9f1a lea      rcx, [rip + 0x2bb591f]
1805e9f21 call     0x180309af0 ; 
1805e9f26 lea      rcx, [rip + 0x2bc44fb]
1805e9f2d call     0x180309af0 ; 
1805e9f32 lea      rcx, [rip + 0x2ba09ff]
1805e9f39 call     0x180309af0 ; 
1805e9f3e lea      rcx, [rip + 0x2bd2773]
1805e9f45 call     0x180309af0 ; 
1805e9f4a lea      rcx, [rip + 0x2bafe87]
1805e9f51 call     0x180309af0 ; 
1805e9f56 lea      rcx, [rip + 0x2bc7c63]
1805e9f5d call     0x180309af0 ; 
1805e9f62 mov      byte ptr [rip + 0x2d53dfe], 1
1805e9f69 mov      rcx, qword ptr [rip + 0x2bd2748]
1805e9f70 call     0x180309ce0 ; 
1805e9f75 xor      edx, edx
1805e9f77 mov      rcx, rax
1805e9f7a mov      rdi, rax
1805e9f7d call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1805e9f82 test     rdi, rdi
1805e9f85 je       0x1805ea085
1805e9f8b mov      qword ptr [rdi + 0x10], rsi
1805e9f8f mov      qword ptr [rdi + 0x18], rbx
1805e9f93 test     rbx, rbx
1805e9f96 je       0x1805ea085
1805e9f9c cmp      dword ptr [rbx + 0x20], 0
1805e9fa0 jle      0x1805ea075
1805e9fa6 mov      r8, qword ptr [rip + 0x2bb575b]
1805e9fad lea      rcx, [rsp + 0x40]
1805e9fb2 mov      rdx, rbx
1805e9fb5 call     0x180d04db0 ; 
1805e9fba xor      edx, edx
1805e9fbc mov      rcx, rsi
1805e9fbf call     0x1821f90c0 ; 5232:UnityEngine.Component.get_gameObject
1805e9fc4 test     rax, rax
1805e9fc7 je       0x1805ea085
1805e9fcd xor      edx, edx
1805e9fcf mov      rcx, rax
1805e9fd2 call     0x1822044d0 ; 5245:UnityEngine.GameObject.get_scene
1805e9fd7 mov      rcx, qword ptr [rip + 0x2bafdfa]
1805e9fde mov      ebx, eax
1805e9fe0 call     0x180309ce0 ; 
1805e9fe5 mov      r8, qword ptr [rip + 0x2ba094c]
1805e9fec xor      r9d, r9d
1805e9fef mov      rdx, rdi
1805e9ff2 mov      rcx, rax
1805e9ff5 mov      rsi, rax
1805e9ff8 call     0x1805672f0 ; 3297:._oA..ctor | 3305:._RA..ctor | 3695:._DC..ctor | 3754:._XC..ctor
1805e9ffd mov      rcx, qword ptr [rip + 0x2bc7bbc]
1805ea004 cmp      dword ptr [rcx + 0xe4], 0
1805ea00b jne      0x1805ea012
1805ea00d call     0x180309de0 ; 
1805ea012 movzx    ecx, byte ptr [rsp + 0x40]
1805ea017 xor      eax, eax
1805ea019 mov      qword ptr [rsp + 0x38], rax
1805ea01e xor      r9d, r9d
1805ea021 mov      qword ptr [rsp + 0x30], rax
1805ea026 xorps    xmm2, xmm2
1805ea029 mov      qword ptr [rsp + 0x28], rsi
1805ea02e mov      edx, ebx
1805ea030 mov      qword ptr [rsp + 0x20], rax
1805ea035 call     0x18058aad0 ; 3757:_d._wC._cs
1805ea03a mov      rax, qword ptr [rip + 0x2bc43e7]
1805ea041 cmp      dword ptr [rax + 0xe4], 0
1805ea048 jne      0x1805ea059
1805ea04a mov      rcx, rax
1805ea04d call     0x180309de0 ; 
1805ea052 mov      rax, qword ptr [rip + 0x2bc43cf]
1805ea059 mov      rax, qword ptr [rax + 0xb8]
1805ea060 mov      rcx, qword ptr [rax]
1805ea063 test     rcx, rcx
1805ea066 je       0x1805ea085
1805ea068 mov      rdx, qword ptr [rsp + 0x48]
1805ea06d xor      r8d, r8d
1805ea070 call     0x1804d4080 ; 9751:_K._NH._woA
1805ea075 mov      rbx, qword ptr [rsp + 0x60]
1805ea07a mov      rsi, qword ptr [rsp + 0x68]
1805ea07f add      rsp, 0x50
1805ea083 pop      rdi
1805ea084 ret      
1805ea085 call     0x180309d40 ; 

FUNCTION 3946 CraftingConfirmationLayer _QY 0x1805e99d0 768
1805e99d0 mov      qword ptr [rsp + 0x20], rbp
1805e99d5 push     rdi
1805e99d6 sub      rsp, 0x50
1805e99da cmp      byte ptr [rip + 0x2d5438c], 0
1805e99e1 mov      ebp, edx
1805e99e3 mov      rdi, rcx
1805e99e6 jne      0x1805e9a13
1805e99e8 lea      rcx, [rip + 0x2bca2f9]
1805e99ef call     0x180309af0 ; 
1805e99f4 lea      rcx, [rip + 0x2ba92c5]
1805e99fb call     0x180309af0 ; 
1805e9a00 lea      rcx, [rip + 0x2bc8579]
1805e9a07 call     0x180309af0 ; 
1805e9a0c mov      byte ptr [rip + 0x2d5435a], 1
1805e9a13 cmp      dword ptr [rdi + 0x184], 0
1805e9a1a mov      qword ptr [rsp + 0x60], rbx
1805e9a1f mov      qword ptr [rsp + 0x68], rsi
1805e9a24 mov      qword ptr [rsp + 0x70], r14
1805e9a29 jl       0x1805e9c14
1805e9a2f cmp      byte ptr [rip + 0x2d54263], 0
1805e9a36 jne      0x1805e9a4b
1805e9a38 lea      rcx, [rip + 0x2bd0f09]
1805e9a3f call     0x180309af0 ; 
1805e9a44 mov      byte ptr [rip + 0x2d5424e], 1
1805e9a4b movsxd   rax, dword ptr [rdi + 0x184]
1805e9a52 cmp      dword ptr [rdi + rax*4 + 0x170], ebp
1805e9a59 je       0x1805e9c37
1805e9a5f xor      esi, esi
1805e9a61 nop      dword ptr [rax]
1805e9a65 nop      word ptr [rax + rax]
1805e9a70 mov      rdx, qword ptr [rdi + 0x108]
1805e9a77 test     rdx, rdx
1805e9a7a je       0x1805e9cb8
1805e9a80 mov      r9, qword ptr [rip + 0x2bca261]
1805e9a87 lea      rcx, [rsp + 0x30]
1805e9a8c mov      ebx, esi
1805e9a8e mov      r8d, ebp
1805e9a91 shl      rbx, 5
1805e9a95 movzx    ebx, word ptr [rdi + rbx + 0x128]
1805e9a9d call     0x180a71ac0 ; 
1805e9aa2 mov      rcx, qword ptr [rip + 0x2ba9217]
1805e9aa9 cmp      dword ptr [rcx + 0xe4], 0
1805e9ab0 jne      0x1805e9ab7
1805e9ab2 call     0x180309de0 ; 
1805e9ab7 movzx    edx, word ptr [rsp + 0x48]
1805e9abc xor      r8d, r8d
1805e9abf movzx    ecx, bx
1805e9ac2 call     0x1804cbd40 ; 9785:ifapp.Game.Data.TraitId.op_Equality
1805e9ac7 test     al, al
1805e9ac9 je       0x1805e9aef
1805e9acb xorps    xmm0, xmm0
1805e9ace mov      eax, esi
1805e9ad0 shl      rax, 5
1805e9ad4 movups   xmmword ptr [rax + rdi + 0x110], xmm0
1805e9adc movups   xmmword ptr [rax + rdi + 0x120], xmm0
1805e9ae4 mov      dword ptr [rdi + rsi*4 + 0x170], 0xffffffff
1805e9aef inc      esi
1805e9af1 cmp      esi, 3
1805e9af4 jl       0x1805e9a70
1805e9afa movsxd   rbx, dword ptr [rdi + 0x184]
1805e9b01 cmp      ebx, 3
1805e9b04 jae      0x1805e9cbe
1805e9b0a cmp      qword ptr [rdi + 0x30], 0
1805e9b0f je       0x1805e9cb8
1805e9b15 cmp      byte ptr [rip + 0x2d53ac9], 0
1805e9b1c jne      0x1805e9b31
1805e9b1e lea      rcx, [rip + 0x2ba88db]
1805e9b25 call     0x180309af0 ; 
1805e9b2a mov      byte ptr [rip + 0x2d53ab4], 1
1805e9b31 mov      rax, qword ptr [rip + 0x2ba88c8]
1805e9b38 cmp      dword ptr [rax + 0xe4], 0
1805e9b3f jne      0x1805e9b50
1805e9b41 mov      rcx, rax
1805e9b44 call     0x180309de0 ; 
1805e9b49 mov      rax, qword ptr [rip + 0x2ba88b0]
1805e9b50 mov      rdx, qword ptr [rdi + 0x108]
1805e9b57 test     rdx, rdx
1805e9b5a je       0x1805e9cb8
1805e9b60 mov      rax, qword ptr [rax + 0xb8]
1805e9b67 lea      rcx, [rsp + 0x30]
1805e9b6c mov      r9, qword ptr [rip + 0x2bca175]
1805e9b73 mov      r8d, ebp
1805e9b76 mov      rsi, qword ptr [rax + 0x18]
1805e9b7a call     0x180a71ac0 ; 
1805e9b7f test     rsi, rsi
1805e9b82 je       0x1805e9cb8
1805e9b88 movzx    edx, word ptr [rsp + 0x48]
1805e9b8d xor      r8d, r8d
1805e9b90 mov      rcx, rsi
1805e9b93 call     0x1804938b0 ; 9619:ifapp.Game.Data.GameData._rmA
1805e9b98 mov      rcx, rbx
1805e9b9b shl      rcx, 5
1805e9b9f movups   xmm0, xmmword ptr [rax]
1805e9ba2 movups   xmm1, xmmword ptr [rax + 0x10]
1805e9ba6 movups   xmmword ptr [rcx + rdi + 0x110], xmm0
1805e9bae movups   xmmword ptr [rcx + rdi + 0x120], xmm1
1805e9bb6 movsxd   rax, dword ptr [rdi + 0x184]
1805e9bbd mov      dword ptr [rdi + rax*4 + 0x170], ebp
1805e9bc4 mov      rax, qword ptr [rip + 0x2bc83b5]
1805e9bcb cmp      dword ptr [rax + 0xe4], 0
1805e9bd2 jne      0x1805e9be3
1805e9bd4 mov      rcx, rax
1805e9bd7 call     0x180309de0 ; 
1805e9bdc mov      rax, qword ptr [rip + 0x2bc839d]
1805e9be3 mov      rax, qword ptr [rax + 0xb8]
1805e9bea mov      rcx, qword ptr [rax]
1805e9bed test     rcx, rcx
1805e9bf0 je       0x1805e9cb8
1805e9bf6 movss    xmm3, dword ptr [rip + 0x201d7be]
1805e9bfe mov      r8d, 8
1805e9c04 mov      qword ptr [rsp + 0x20], 0
1805e9c0d mov      dl, 0x1e
1805e9c0f call     0x180452720 ; 11610:_J._zf._oFA
1805e9c14 xor      edx, edx
1805e9c16 mov      rcx, rdi
1805e9c19 mov      r14, qword ptr [rsp + 0x70]
1805e9c1e mov      rsi, qword ptr [rsp + 0x68]
1805e9c23 mov      rbx, qword ptr [rsp + 0x60]
1805e9c28 mov      rbp, qword ptr [rsp + 0x78]
1805e9c2d add      rsp, 0x50
1805e9c31 pop      rdi
1805e9c32 jmp      0x1805e9d40 ; 3946:ifapp.Game.Scenes.Recipe.ConfirmationLayer.CraftingConfirmationLayer._nY
1805e9c37 movsxd   rax, dword ptr [rdi + 0x184]
1805e9c3e cmp      eax, 3
1805e9c41 jae      0x1805e9cbe
1805e9c43 shl      rax, 5
1805e9c47 xorps    xmm0, xmm0
1805e9c4a movups   xmmword ptr [rax + rdi + 0x110], xmm0
1805e9c52 movups   xmmword ptr [rax + rdi + 0x120], xmm0
1805e9c5a movsxd   rax, dword ptr [rdi + 0x184]
1805e9c61 mov      dword ptr [rdi + rax*4 + 0x170], 0xffffffff
1805e9c6c mov      rax, qword ptr [rip + 0x2bc830d]
1805e9c73 cmp      dword ptr [rax + 0xe4], 0
1805e9c7a jne      0x1805e9c8b
1805e9c7c mov      rcx, rax
1805e9c7f call     0x180309de0 ; 
1805e9c84 mov      rax, qword ptr [rip + 0x2bc82f5]
1805e9c8b mov      rax, qword ptr [rax + 0xb8]
1805e9c92 mov      rcx, qword ptr [rax]
1805e9c95 test     rcx, rcx
1805e9c98 je       0x1805e9cb8
1805e9c9a movss    xmm3, dword ptr [rip + 0x201d71a]
1805e9ca2 mov      r8d, 8
1805e9ca8 mov      qword ptr [rsp + 0x20], 0
1805e9cb1 mov      dl, 0x1f
1805e9cb3 jmp      0x1805e9c0f ; 
1805e9cb8 call     0x180309d40 ; 
1805e9cbe call     0x180309d30 ; 

FUNCTION 3948 CraftingConfirmationLeftContainer _wY 0x1805eaf20 208
1805eaf20 mov      qword ptr [rsp + 8], rbx
1805eaf25 mov      qword ptr [rsp + 0x10], rsi
1805eaf2a push     rdi
1805eaf2b sub      rsp, 0x20
1805eaf2f cmp      byte ptr [rip + 0x2d52e3b], 0
1805eaf36 movzx    ebx, r9b
1805eaf3a mov      rdi, rdx
1805eaf3d mov      rsi, rcx
1805eaf40 jne      0x1805eaf55
1805eaf42 lea      rcx, [rip + 0x2ba7d77]
1805eaf49 call     0x180309af0 ; 
1805eaf4e mov      byte ptr [rip + 0x2d52e1c], 1
1805eaf55 test     rdi, rdi
1805eaf58 je       0x1805eafe4
1805eaf5e test     bl, bl
1805eaf60 jne      0x1805eafb9
1805eaf62 mov      rcx, qword ptr [rip + 0x2ba7d57]
1805eaf69 movzx    ebx, word ptr [rsi + 0x80]
1805eaf70 movzx    edi, word ptr [rdi + 0x6c]
1805eaf74 cmp      dword ptr [rcx + 0xe4], 0
1805eaf7b jne      0x1805eaf82
1805eaf7d call     0x180309de0 ; 
1805eaf82 xor      r8d, r8d
1805eaf85 movzx    edx, di
1805eaf88 movzx    ecx, bx
1805eaf8b call     0x1804cbd40 ; 9785:ifapp.Game.Data.TraitId.op_Equality
1805eaf90 test     al, al
1805eaf92 je       0x1805eafcb
1805eaf94 mov      rcx, qword ptr [rip + 0x2ba7d25]
1805eaf9b cmp      dword ptr [rcx + 0xe4], 0
1805eafa2 jne      0x1805eafa9
1805eafa4 call     0x180309de0 ; 
1805eafa9 xor      ecx, ecx
1805eafab call     0x180490f40 ; 107:Mono.Security.Cryptography.DSAManaged.get_KeyExchangeAlgorithm | 328:System.Type.get_DeclaringType | 328:System.Type.get_DeclaringMethod | 328:System.Type.get_ReflectedType
1805eafb0 mov      byte ptr [rsi + 0x82], 0
1805eafb7 jmp      0x1805eafc4 ; 
1805eafb9 movzx    eax, word ptr [rdi + 0x6c]
1805eafbd mov      byte ptr [rsi + 0x82], 1
1805eafc4 mov      word ptr [rsi + 0x80], ax
1805eafcb xor      edx, edx
1805eafcd mov      rcx, rsi
1805eafd0 mov      rbx, qword ptr [rsp + 0x30]
1805eafd5 mov      rsi, qword ptr [rsp + 0x38]
1805eafda add      rsp, 0x20
1805eafde pop      rdi
1805eafdf jmp      0x1805ea890 ; 3948:ifapp.Game.Scenes.Recipe.ConfirmationLayer.CraftingConfirmationLeftContainer._WY
1805eafe4 call     0x180309d40 ; 

FUNCTION 9631 GameplayHistory _BMA 0x180494130 16
180494130 movzx    eax, cl
180494133 sub      eax, 8
180494136 cmp      eax, 2
180494139 setbe    al
18049413c ret      

FUNCTION 9627 _Sh .cctor 0x18049a740 176
18049a740 sub      rsp, 0x58
18049a744 cmp      byte ptr [rip + 0x2ea2e91], 0
18049a74b jne      0x18049a760
18049a74d lea      rcx, [rip + 0x2d144f4]
18049a754 call     0x180309af0 ; 
18049a759 mov      byte ptr [rip + 0x2ea2e7c], 1
18049a760 mov      rax, qword ptr [rip + 0x2d144e1]
18049a767 xorps    xmm1, xmm1
18049a76a movups   xmmword ptr [rsp + 0x20], xmm1
18049a76f mov      byte ptr [rsp + 0x20], 8
18049a774 xorps    xmm2, xmm2
18049a777 mov      rcx, qword ptr [rax + 0xb8]
18049a77e movups   xmm0, xmmword ptr [rsp + 0x20]
18049a783 movups   xmmword ptr [rsp + 0x20], xmm2
18049a788 mov      byte ptr [rsp + 0x20], 0xd
18049a78d movups   xmmword ptr [rcx], xmm0
18049a790 movups   xmm0, xmmword ptr [rsp + 0x20]
18049a795 movups   xmmword ptr [rcx + 0x10], xmm1
18049a799 movups   xmmword ptr [rcx + 0x20], xmm1
18049a79d mov      rax, qword ptr [rip + 0x2d144a4]
18049a7a4 movups   xmmword ptr [rsp + 0x20], xmm1
18049a7a9 mov      byte ptr [rsp + 0x20], 0xe
18049a7ae mov      rcx, qword ptr [rax + 0xb8]
18049a7b5 movups   xmmword ptr [rcx + 0x30], xmm0
18049a7b9 movups   xmm0, xmmword ptr [rsp + 0x20]
18049a7be movups   xmmword ptr [rcx + 0x40], xmm2
18049a7c2 movups   xmmword ptr [rcx + 0x50], xmm2
18049a7c6 mov      rax, qword ptr [rip + 0x2d1447b]
18049a7cd mov      rcx, qword ptr [rax + 0xb8]
18049a7d4 movups   xmmword ptr [rcx + 0x60], xmm0
18049a7d8 movups   xmmword ptr [rcx + 0x70], xmm1
18049a7dc movups   xmmword ptr [rcx + 0x80], xmm1
18049a7e3 add      rsp, 0x58
18049a7e7 ret      

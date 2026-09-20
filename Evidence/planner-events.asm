
FUNCTION 9623 _qh .ctor 0x180431940 16
180431940 xor      edx, edx
180431942 jmp      0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing

FUNCTION 9623 _qh Compare 0x18049d330 16
18049d330 mov      eax, dword ptr [rdx]
18049d332 sub      eax, dword ptr [r8]
18049d335 ret      

FUNCTION 9624 _Ph .cctor 0x18049a380 384
18049a380 push     rbx
18049a382 sub      rsp, 0x20
18049a386 cmp      byte ptr [rip + 0x2ea324a], 0
18049a38d jne      0x18049a3de
18049a38f lea      rcx, [rip + 0x2cd447a]
18049a396 call     0x180309af0 ; 
18049a39b lea      rcx, [rip + 0x2d04fce]
18049a3a2 call     0x180309af0 ; 
18049a3a7 lea      rcx, [rip + 0x2d05712]
18049a3ae call     0x180309af0 ; 
18049a3b3 lea      rcx, [rip + 0x2d05be6]
18049a3ba call     0x180309af0 ; 
18049a3bf lea      rcx, [rip + 0x2d14422]
18049a3c6 call     0x180309af0 ; 
18049a3cb lea      rcx, [rip + 0x2cfd5ee]
18049a3d2 call     0x180309af0 ; 
18049a3d7 mov      byte ptr [rip + 0x2ea31f9], 1
18049a3de mov      rcx, qword ptr [rip + 0x2cfd5db]
18049a3e5 call     0x180309ce0 ; 
18049a3ea xor      edx, edx
18049a3ec mov      rcx, rax
18049a3ef mov      rbx, rax
18049a3f2 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
18049a3f7 mov      rcx, qword ptr [rip + 0x2d143ea]
18049a3fe mov      rdx, qword ptr [rcx + 0xb8]
18049a405 mov      qword ptr [rdx], rbx
18049a408 mov      edx, 6
18049a40d mov      rcx, qword ptr [rip + 0x2cd43fc]
18049a414 call     0x180308ef0 ; 
18049a419 mov      rdx, qword ptr [rip + 0x2d05b80]
18049a420 xor      r8d, r8d
18049a423 mov      rcx, rax
18049a426 mov      rbx, rax
18049a429 call     0x181811ef0 ; 1075:System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray
18049a42e mov      rcx, qword ptr [rip + 0x2d143b3]
18049a435 mov      rdx, qword ptr [rcx + 0xb8]
18049a43c mov      qword ptr [rdx + 8], rbx
18049a440 mov      edx, 0xb
18049a445 mov      rcx, qword ptr [rip + 0x2cd43c4]
18049a44c call     0x180308ef0 ; 
18049a451 mov      rdx, qword ptr [rip + 0x2d04f18]
18049a458 xor      r8d, r8d
18049a45b mov      rcx, rax
18049a45e mov      rbx, rax
18049a461 call     0x181811ef0 ; 1075:System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray
18049a466 mov      rcx, qword ptr [rip + 0x2d1437b]
18049a46d mov      edx, 0xb
18049a472 mov      r8, qword ptr [rcx + 0xb8]
18049a479 mov      qword ptr [r8 + 0x10], rbx
18049a47d mov      rcx, qword ptr [rip + 0x2cd438c]
18049a484 call     0x180308ef0 ; 
18049a489 mov      rdx, qword ptr [rip + 0x2d05630]
18049a490 xor      r8d, r8d
18049a493 mov      rcx, rax
18049a496 mov      rbx, rax
18049a499 call     0x181811ef0 ; 1075:System.Runtime.CompilerServices.RuntimeHelpers.InitializeArray
18049a49e mov      rcx, qword ptr [rip + 0x2d14343]
18049a4a5 mov      rdx, qword ptr [rcx + 0xb8]
18049a4ac mov      qword ptr [rdx + 0x18], rbx
18049a4b0 mov      edx, 2
18049a4b5 mov      rcx, qword ptr [rip + 0x2cd4354]
18049a4bc call     0x180308ef0 ; 
18049a4c1 mov      rdx, rax
18049a4c4 test     rax, rax
18049a4c7 je       0x18049a4ee
18049a4c9 cmp      dword ptr [rax + 0x18], 1
18049a4cd jbe      0x18049a4f4
18049a4cf mov      dword ptr [rax + 0x24], 0x64
18049a4d6 mov      rax, qword ptr [rip + 0x2d1430b]
18049a4dd mov      rcx, qword ptr [rax + 0xb8]
18049a4e4 mov      qword ptr [rcx + 0x20], rdx
18049a4e8 add      rsp, 0x20
18049a4ec pop      rbx
18049a4ed ret      
18049a4ee call     0x180309d40 ; 
18049a4f4 call     0x180309d30 ; 

FUNCTION 9633 _uh _EMA 0x180490f40 16
180490f40 xor      eax, eax
180490f42 ret      

FUNCTION 9633 _uh _JMA 0x1804bbff0 16
1804bbff0 mov      eax, 0xa
1804bbff5 ret      

FUNCTION 9633 _uh _kMA 0x1804bc0f0 16
1804bc0f0 movzx    eax, word ptr [rcx]
1804bc0f3 add      edx, edx
1804bc0f5 or       edx, 1
1804bc0f8 cmp      edx, eax
1804bc0fa sete     al
1804bc0fd ret      

FUNCTION 9633 _uh _KMA 0x1804bc000 96
1804bc000 test     edx, edx
1804bc002 je       0x1804bc04d
1804bc004 sub      edx, 1
1804bc007 je       0x1804bc03f
1804bc009 sub      edx, 1
1804bc00c je       0x1804bc031
1804bc00e sub      edx, 1
1804bc011 je       0x1804bc023
1804bc013 cmp      edx, 1
1804bc016 je       0x1804bc01b
1804bc018 xor      al, al
1804bc01a ret      
1804bc01b cmp      word ptr [rcx], 9
1804bc01f sete     al
1804bc022 ret      
1804bc023 cmp      word ptr [rcx], 7
1804bc027 je       0x1804bc053
1804bc029 cmp      word ptr [rcx], 8
1804bc02d sete     al
1804bc030 ret      
1804bc031 cmp      word ptr [rcx], 5
1804bc035 je       0x1804bc053
1804bc037 cmp      word ptr [rcx], 6
1804bc03b sete     al
1804bc03e ret      
1804bc03f cmp      word ptr [rcx], 3
1804bc043 je       0x1804bc053
1804bc045 cmp      word ptr [rcx], 4
1804bc049 sete     al
1804bc04c ret      
1804bc04d cmp      word ptr [rcx], 1
1804bc051 jne      0x1804bc056
1804bc053 mov      al, 1
1804bc055 ret      
1804bc056 cmp      word ptr [rcx], 2
1804bc05a sete     al
1804bc05d ret      

FUNCTION 9633 _uh _lMA 0x1804bc100 16
1804bc100 lea      eax, [rcx + rcx]
1804bc103 or       ax, 1
1804bc107 ret      

FUNCTION 9633 _uh _LMA 0x1804bc060 16
1804bc060 lea      ax, [rcx*2 + 2]
1804bc068 ret      

FUNCTION 9633 _uh _mMA 0x1804bc110 128
1804bc110 movzx    eax, word ptr [rcx]
1804bc113 dec      eax
1804bc115 cmp      eax, 8
1804bc118 ja       0x1804bc15d
1804bc11a lea      r8, [rip - 0x4bc121]
1804bc121 cdqe     
1804bc123 mov      ecx, dword ptr [r8 + rax*4 + 0x4bc168]
1804bc12b add      rcx, r8
1804bc12e jmp      rcx
1804bc130 mov      dword ptr [rdx], 0
1804bc136 mov      al, 1
1804bc138 ret      
1804bc139 mov      dword ptr [rdx], 1
1804bc13f mov      al, 1
1804bc141 ret      
1804bc142 mov      dword ptr [rdx], 2
1804bc148 mov      al, 1
1804bc14a ret      
1804bc14b mov      dword ptr [rdx], 3
1804bc151 mov      al, 1
1804bc153 ret      
1804bc154 mov      dword ptr [rdx], 4
1804bc15a mov      al, 1
1804bc15c ret      
1804bc15d mov      dword ptr [rdx], 0xffffffff
1804bc163 xor      al, al
1804bc165 ret      
1804bc166 nop      
1804bc168 xor      cl, al
1804bc16a add      byte ptr [r13 - 0x3f], bl
1804bc16e add      byte ptr [r9], dil
1804bc171 ror      dword ptr [rbx], 0x5d
1804bc175 ror      dword ptr [rbx], 0x42
1804bc179 ror      dword ptr [rbx], 0x5d
1804bc17d ror      dword ptr [rbx], 0x4b
1804bc181 ror      dword ptr [rbx], 0x5d
1804bc185 ror      dword ptr [rbx], 0x54
1804bc189 ror      dword ptr [rbx], 0xcc

FUNCTION 9633 _uh _MMA 0x1804bc070 128
1804bc070 movzx    eax, word ptr [rcx]
1804bc073 add      eax, -2
1804bc076 cmp      eax, 8
1804bc079 ja       0x1804bc0be
1804bc07b lea      r8, [rip - 0x4bc082]
1804bc082 cdqe     
1804bc084 mov      ecx, dword ptr [r8 + rax*4 + 0x4bc0c8]
1804bc08c add      rcx, r8
1804bc08f jmp      rcx
1804bc091 mov      dword ptr [rdx], 0
1804bc097 mov      al, 1
1804bc099 ret      
1804bc09a mov      dword ptr [rdx], 1
1804bc0a0 mov      al, 1
1804bc0a2 ret      
1804bc0a3 mov      dword ptr [rdx], 2
1804bc0a9 mov      al, 1
1804bc0ab ret      
1804bc0ac mov      dword ptr [rdx], 3
1804bc0b2 mov      al, 1
1804bc0b4 ret      
1804bc0b5 mov      dword ptr [rdx], 4
1804bc0bb mov      al, 1
1804bc0bd ret      
1804bc0be mov      dword ptr [rdx], 0xffffffff
1804bc0c4 xor      al, al
1804bc0c6 ret      
1804bc0c7 nop      
1804bc0c8 xchg     ecx, eax
1804bc0c9 ror      byte ptr [rbx], 0xbe
1804bc0cd ror      byte ptr [rbx], 0x9a
1804bc0d1 ror      byte ptr [rbx], 0xbe
1804bc0d5 ror      byte ptr [rbx], 0xa3
1804bc0d9 ror      byte ptr [rbx], 0xbe
1804bc0dd ror      byte ptr [rbx], 0xac
1804bc0e1 ror      byte ptr [rbx], 0xbe
1804bc0e5 ror      byte ptr [rbx], 0xb5
1804bc0e9 ror      byte ptr [rbx], 0xcc

FUNCTION 9633 _uh Equals 0x1804b12b0 16
1804b12b0 cmp      word ptr [rcx], dx
1804b12b3 sete     al
1804b12b6 ret      

FUNCTION 9633 _uh Equals 0x1804bbf50 144
1804bbf50 mov      qword ptr [rsp + 8], rbx
1804bbf55 push     rdi
1804bbf56 sub      rsp, 0x20
1804bbf5a cmp      byte ptr [rip + 0x2e8169b], 0
1804bbf61 mov      rbx, rdx
1804bbf64 mov      rdi, rcx
1804bbf67 jne      0x1804bbf7c
1804bbf69 lea      rcx, [rip + 0x2cf59d0]
1804bbf70 call     0x180309af0 ; 
1804bbf75 mov      byte ptr [rip + 0x2e81680], 1
1804bbf7c test     rbx, rbx
1804bbf7f je       0x1804bbfbf
1804bbf81 mov      rdx, qword ptr [rip + 0x2cf59b8]
1804bbf88 xor      eax, eax
1804bbf8a cmp      qword ptr [rbx], rdx
1804bbf8d cmove    rax, rbx
1804bbf91 test     rax, rax
1804bbf94 je       0x1804bbfbf
1804bbf96 mov      rcx, qword ptr [rbx]
1804bbf99 mov      rax, qword ptr [rdx + 0x40]
1804bbf9d cmp      qword ptr [rcx + 0x40], rax
1804bbfa1 mov      rcx, rbx
1804bbfa4 jne      0x1804bbfcc
1804bbfa6 call     0x180309020 ; 
1804bbfab movzx    ecx, word ptr [rax]
1804bbfae cmp      word ptr [rdi], cx
1804bbfb1 sete     al
1804bbfb4 mov      rbx, qword ptr [rsp + 0x30]
1804bbfb9 add      rsp, 0x20
1804bbfbd pop      rdi
1804bbfbe ret      
1804bbfbf mov      rbx, qword ptr [rsp + 0x30]
1804bbfc4 xor      al, al
1804bbfc6 add      rsp, 0x20
1804bbfca pop      rdi
1804bbfcb ret      
1804bbfcc call     0x180308eb0 ; 

FUNCTION 9633 _uh GetHashCode 0x1804bbfe0 16
1804bbfe0 xor      edx, edx
1804bbfe2 jmp      0x1804b12c0 ; 196:System.Char.System.IConvertible.ToChar | 255:System.Int16.System.IConvertible.ToInt16 | 332:System.UInt16.GetHashCode | 332:System.UInt16.System.IConvertible.ToUInt16

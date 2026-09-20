
FUNCTION 11600 StringsMapping GetCurrent 0x181a001c0 80
181a001c0 sub      rsp, 0x28
181a001c4 cmp      byte ptr [rip + 0x1942d2b], 0
181a001cb jne      0x181a001ec
181a001cd lea      rcx, [rip + 0x17ccdec]
181a001d4 call     0x180309af0 ; 
181a001d9 lea      rcx, [rip + 0x17b13f8]
181a001e0 call     0x180309af0 ; 
181a001e5 mov      byte ptr [rip + 0x1942d0a], 1
181a001ec mov      rdx, qword ptr [rip + 0x17ccdcd]
181a001f3 mov      rcx, qword ptr [rip + 0x17b13de]
181a001fa add      rsp, 0x28
181a001fe jmp      0x18094de80 ; 

FUNCTION 11600 StringsMapping .ctor 0x180458b60 16
180458b60 xor      edx, edx
180458b62 jmp      0x182211640 ; 5258:UnityEngine.ScriptableObject..ctor

FUNCTION 9576 _Dh _CLA 0x180497180 16
180497180 cmp      byte ptr [rcx + 0x20], 0
180497184 sete     al
180497187 ret      

FUNCTION 9576 _Dh _dLA 0x180497190 16
180497190 cmp      byte ptr [rcx + 0x21], 0
180497194 sete     al
180497197 ret      

FUNCTION 9704 _dH _pnA 0x1804bb7f0 16
1804bb7f0 movsd    xmm0, qword ptr [rcx + 0x18]
1804bb7f5 subsd    xmm0, qword ptr [rcx + 0x10]
1804bb7fa ret      

FUNCTION 9704 _dH _PnA 0x1804bb700 32
1804bb700 movsd    xmm0, qword ptr [rcx + 0x18]
1804bb705 movsd    xmm1, qword ptr [rcx + 8]
1804bb70a subsd    xmm0, qword ptr [rcx + 0x10]
1804bb70f subsd    xmm1, qword ptr [rcx]
1804bb713 subsd    xmm0, xmm1
1804bb717 ret      

FUNCTION 9704 _dH _qnA 0x1804bb800 16
1804bb800 movsd    xmm0, qword ptr [rcx + 8]
1804bb805 subsd    xmm0, qword ptr [rcx]
1804bb809 ret      

FUNCTION 9704 _dH _QnA 0x1804bb720 208
1804bb720 push     rbx
1804bb722 sub      rsp, 0x60
1804bb726 cmp      byte ptr [rip + 0x2e81f5f], 0
1804bb72d mov      rbx, rcx
1804bb730 movaps   xmmword ptr [rsp + 0x50], xmm6
1804bb735 movaps   xmmword ptr [rsp + 0x40], xmm7
1804bb73a movaps   xmm7, xmm1
1804bb73d movaps   xmmword ptr [rsp + 0x30], xmm8
1804bb743 movaps   xmmword ptr [rsp + 0x20], xmm9
1804bb749 movaps   xmm9, xmm2
1804bb74d jne      0x1804bb762
1804bb74f lea      rcx, [rip + 0x2d0af6a]
1804bb756 call     0x180309af0 ; 
1804bb75b mov      byte ptr [rip + 0x2e81f2a], 1
1804bb762 mov      rcx, qword ptr [rip + 0x2d0af57]
1804bb769 movsd    xmm6, qword ptr [rbx + 0x10]
1804bb76e movsd    xmm8, qword ptr [rbx + 0x18]
1804bb774 cmp      dword ptr [rcx + 0xe4], 0
1804bb77b jne      0x1804bb782
1804bb77d call     0x180309de0 ; 
1804bb782 addsd    xmm6, xmm9
1804bb787 xor      r8d, r8d
1804bb78a movaps   xmm1, xmm8
1804bb78e movaps   xmm0, xmm6
1804bb791 call     0x1818f7200 ; 266:System.Math.Min
1804bb796 addsd    xmm7, qword ptr [rbx]
1804bb79a movsd    xmm1, qword ptr [rbx + 8]
1804bb79f movaps   xmm6, xmm0
1804bb7a2 xor      r8d, r8d
1804bb7a5 movaps   xmm0, xmm7
1804bb7a8 call     0x1818f7200 ; 266:System.Math.Min
1804bb7ad movsd    xmm2, qword ptr [rbx + 0x18]
1804bb7b2 movsd    xmm1, qword ptr [rbx + 8]
1804bb7b7 subsd    xmm2, xmm6
1804bb7bb movaps   xmm6, xmmword ptr [rsp + 0x50]
1804bb7c0 subsd    xmm1, xmm0
1804bb7c4 movaps   xmm7, xmmword ptr [rsp + 0x40]
1804bb7c9 movaps   xmm8, xmmword ptr [rsp + 0x30]
1804bb7cf movaps   xmm9, xmmword ptr [rsp + 0x20]
1804bb7d5 subsd    xmm2, xmm1
1804bb7d9 movaps   xmm0, xmm2
1804bb7dc add      rsp, 0x60
1804bb7e0 pop      rbx
1804bb7e1 ret      

FUNCTION 9708 _DH _rnA 0x1804b77d0 32
1804b77d0 lea      eax, [rdx + 0xfc]
1804b77d6 test     al, 0xfd
1804b77d8 jne      0x1804b77dd
1804b77da mov      al, 1
1804b77dc ret      
1804b77dd cmp      dl, 5
1804b77e0 sete     al
1804b77e3 ret      

FUNCTION 9708 _DH _RnA 0x1804b76a0 272
1804b76a0 mov      qword ptr [rsp + 0x18], rsi
1804b76a5 push     rdi
1804b76a6 sub      rsp, 0x20
1804b76aa cmp      byte ptr [rip + 0x2e85fdc], 0
1804b76b1 mov      rdi, rdx
1804b76b4 mov      rsi, rcx
1804b76b7 jne      0x1804b76cc
1804b76b9 lea      rcx, [rip + 0x2d0f000]
1804b76c0 call     0x180309af0 ; 
1804b76c5 mov      byte ptr [rip + 0x2e85fc1], 1
1804b76cc test     rdi, rdi
1804b76cf je       0x1804b77a6
1804b76d5 mov      qword ptr [rsp + 0x30], rbx
1804b76da xor      ebx, ebx
1804b76dc xor      eax, eax
1804b76de mov      qword ptr [rsp + 0x38], rbp
1804b76e3 cmp      eax, dword ptr [rdi + 0x18]
1804b76e6 jge      0x1804b778b
1804b76ec cmp      ebx, dword ptr [rdi + 0x18]
1804b76ef jae      0x1804b77a0
1804b76f5 movsxd   rax, ebx
1804b76f8 mov      ecx, dword ptr [rdi + rax*4 + 0x20]
1804b76fc mov      eax, 0x66666667
1804b7701 shl      ecx, 2
1804b7704 imul     ecx
1804b7706 mov      ebp, edx
1804b7708 sar      ebp, 1
1804b770a mov      eax, ebp
1804b770c shr      eax, 0x1f
1804b770f add      ebp, eax
1804b7711 movsxd   rax, ebx
1804b7714 mov      word ptr [rsi + rax*2], bp
1804b7718 mov      rcx, qword ptr [rip + 0x2d0efa1]
1804b771f cmp      dword ptr [rcx + 0xe4], 0
1804b7726 jne      0x1804b772d
1804b7728 call     0x180309de0 ; 
1804b772d mov      edx, 1
1804b7732 xor      r8d, r8d
1804b7735 movzx    ecx, bp
1804b7738 call     0x1818f7040 ; 266:System.Math.Max
1804b773d movsxd   rcx, ebx
1804b7740 mov      word ptr [rsi + rcx*2], ax
1804b7744 xor      eax, eax
1804b7746 movsxd   rcx, ebx
1804b7749 mov      word ptr [rsi + rcx*2 + 0xa], ax
1804b774e test     ebx, ebx
1804b7750 jne      0x1804b7762
1804b7752 movsxd   rax, ebx
1804b7755 xor      ecx, ecx
1804b7757 inc      ebx
1804b7759 mov      word ptr [rsi + rax*2 + 0x14], cx
1804b775e mov      eax, ebx
1804b7760 jmp      0x1804b76e3 ; 
1804b7762 movsxd   rdx, ebx
1804b7765 dec      rdx
1804b7768 cmp      edx, dword ptr [rdi + 0x18]
1804b776b jae      0x1804b77a0
1804b776d movsxd   rax, ebx
1804b7770 movzx    ecx, word ptr [rsi + rax*2 + 0x12]
1804b7775 add      cx, word ptr [rdi + rdx*4 + 0x20]
1804b777a movsxd   rax, ebx
1804b777d inc      ebx
1804b777f mov      word ptr [rsi + rax*2 + 0x14], cx
1804b7784 mov      eax, ebx
1804b7786 jmp      0x1804b76e3 ; 
1804b778b mov      rbp, qword ptr [rsp + 0x38]
1804b7790 mov      rbx, qword ptr [rsp + 0x30]
1804b7795 mov      rsi, qword ptr [rsp + 0x40]
1804b779a add      rsp, 0x20
1804b779e pop      rdi
1804b779f ret      
1804b77a0 call     0x180309d30 ; 
1804b77a6 call     0x180309d40 ; 

FUNCTION 9708 _DH _snA 0x1804b77f0 48
1804b77f0 lea      eax, [rdx + 0xfc]
1804b77f6 movsxd   r9, r8d
1804b77f9 test     al, 0xfd
1804b77fb jne      0x1804b7809
1804b77fd mov      edx, 1
1804b7802 add      word ptr [rcx + r9*2 + 0xa], dx
1804b7808 ret      
1804b7809 xor      eax, eax
1804b780b cmp      dl, 5
1804b780e sete     al
1804b7811 add      word ptr [rcx + r9*2 + 0xa], ax
1804b7817 ret      

FUNCTION 9708 _DH _SnA 0x1804b77b0 32
1804b77b0 movsxd   rax, edx
1804b77b3 movsxd   r8, edx
1804b77b6 movzx    edx, word ptr [rcx + rax*2]
1804b77ba cmp      word ptr [rcx + r8*2 + 0xa], dx
1804b77c0 setge    al
1804b77c3 ret      

FUNCTION 9708 _DH _tnA 0x1804b7820 80
1804b7820 movsxd   rax, edx
1804b7823 xorps    xmm1, xmm1
1804b7826 movsx    r8d, word ptr [rcx + rax*2]
1804b782b movd     xmm2, r8d
1804b7830 cvtdq2ps xmm2, xmm2
1804b7833 comiss   xmm2, xmm1
1804b7836 jbe      0x1804b7861
1804b7838 movsxd   rax, edx
1804b783b movsx    ecx, word ptr [rcx + rax*2 + 0xa]
1804b7840 movd     xmm0, ecx
1804b7844 cvtdq2ps xmm0, xmm0
1804b7847 divss    xmm0, xmm2
1804b784b comiss   xmm1, xmm0
1804b784e ja       0x1804b7861
1804b7850 movss    xmm2, dword ptr [rip + 0x214fb64]
1804b7858 comiss   xmm0, xmm2
1804b785b jbe      0x1804b7864
1804b785d movaps   xmm0, xmm2
1804b7860 ret      
1804b7861 xorps    xmm0, xmm0
1804b7864 ret      

FUNCTION 9708 _DH _unA 0x1804b7870 32
1804b7870 movsxd   rax, edx
1804b7873 movsx    r9d, word ptr [rcx + rax*2 + 0x14]
1804b7879 movsxd   rax, edx
1804b787c sub      r8d, r9d
1804b787f movsx    ecx, word ptr [rcx + rax*2]
1804b7883 cmp      r8d, ecx
1804b7886 setge    al
1804b7889 ret      

FUNCTION 9692 IotaInventory get_AllIota 0x1804b0520 144
1804b0520 mov      qword ptr [rsp + 8], rbx
1804b0525 push     rdi
1804b0526 sub      rsp, 0x40
1804b052a cmp      byte ptr [rip + 0x2e8d130], 0
1804b0531 mov      rbx, rdx
1804b0534 mov      rdi, rcx
1804b0537 jne      0x1804b0558
1804b0539 lea      rcx, [rip + 0x2d07060]
1804b0540 call     0x180309af0 ; 
1804b0545 lea      rcx, [rip + 0x2cc4f6c]
1804b054c call     0x180309af0 ; 
1804b0551 mov      byte ptr [rip + 0x2e8d109], 1
1804b0558 mov      rdx, qword ptr [rbx + 0x10]
1804b055c test     rdx, rdx
1804b055f je       0x1804b05a9
1804b0561 mov      r8, qword ptr [rip + 0x2cc4f50]
1804b0568 lea      rcx, [rsp + 0x20]
1804b056d call     0x180fa5990 ; 
1804b0572 movaps   xmm0, xmmword ptr [rsp + 0x20]
1804b0577 lea      rdx, [rsp + 0x20]
1804b057c mov      r8, qword ptr [rip + 0x2d0701d]
1804b0583 lea      rcx, [rsp + 0x30]
1804b0588 movdqa   xmmword ptr [rsp + 0x20], xmm0
1804b058e call     0x180e28f60 ; 
1804b0593 movups   xmm0, xmmword ptr [rsp + 0x30]
1804b0598 mov      rbx, qword ptr [rsp + 0x50]
1804b059d mov      rax, rdi
1804b05a0 movups   xmmword ptr [rdi], xmm0
1804b05a3 add      rsp, 0x40
1804b05a7 pop      rdi
1804b05a8 ret      
1804b05a9 call     0x180309d40 ; 

FUNCTION 9692 IotaInventory OnDeserialized 0x1804aebd0 224
1804aebd0 push     rbx
1804aebd2 sub      rsp, 0x30
1804aebd6 cmp      byte ptr [rip + 0x2e8ea85], 0
1804aebdd mov      rbx, rcx
1804aebe0 jne      0x1804aec19
1804aebe2 lea      rcx, [rip + 0x2d184f7]
1804aebe9 call     0x180309af0 ; 
1804aebee lea      rcx, [rip + 0x2d18583]
1804aebf5 call     0x180309af0 ; 
1804aebfa lea      rcx, [rip + 0x2cc66ef]
1804aec01 call     0x180309af0 ; 
1804aec06 lea      rcx, [rip + 0x2cc677b]
1804aec0d call     0x180309af0 ; 
1804aec12 mov      byte ptr [rip + 0x2e8ea49], 1
1804aec19 mov      rcx, qword ptr [rbx + 0x20]
1804aec1d mov      qword ptr [rsp + 0x40], rsi
1804aec22 mov      qword ptr [rsp + 0x48], rdi
1804aec27 test     rcx, rcx
1804aec2a je       0x1804aeca5
1804aec2c mov      rdx, qword ptr [rip + 0x2d18545]
1804aec33 call     0x181604f10 ; 
1804aec38 mov      rax, qword ptr [rbx + 0x10]
1804aec3c xor      ecx, ecx
1804aec3e xor      edi, edi
1804aec40 test     rax, rax
1804aec43 je       0x1804aeca5
1804aec45 cmp      ecx, dword ptr [rax + 0x18]
1804aec48 jge      0x1804aec95
1804aec4a mov      rcx, qword ptr [rbx + 0x10]
1804aec4e test     rcx, rcx
1804aec51 je       0x1804aeca5
1804aec53 mov      r8, qword ptr [rip + 0x2cc6696]
1804aec5a mov      edx, edi
1804aec5c mov      rsi, qword ptr [rbx + 0x20]
1804aec60 call     0x180fa5910 ; 
1804aec65 test     rsi, rsi
1804aec68 je       0x1804aeca5
1804aec6a movups   xmm0, xmmword ptr [rax]
1804aec6d mov      r8, qword ptr [rip + 0x2d1846c]
1804aec74 lea      rdx, [rsp + 0x20]
1804aec79 mov      rcx, rsi
1804aec7c movaps   xmmword ptr [rsp + 0x20], xmm0
1804aec81 call     0x181619910 ; 
1804aec86 mov      rax, qword ptr [rbx + 0x10]
1804aec8a inc      edi
1804aec8c mov      ecx, edi
1804aec8e test     rax, rax
1804aec91 je       0x1804aeca5
1804aec93 jmp      0x1804aec45 ; 
1804aec95 mov      rdi, qword ptr [rsp + 0x48]
1804aec9a mov      rsi, qword ptr [rsp + 0x40]
1804aec9f add      rsp, 0x30
1804aeca3 pop      rbx
1804aeca4 ret      
1804aeca5 call     0x180309d40 ; 

FUNCTION 9692 IotaInventory Add 0x1804adfd0 800
1804adfd0 mov      qword ptr [rsp + 0x18], rsi
1804adfd5 push     rdi
1804adfd6 sub      rsp, 0x40
1804adfda cmp      byte ptr [rip + 0x2e8f682], 0
1804adfe1 mov      rsi, rdx
1804adfe4 mov      rdi, rcx
1804adfe7 jne      0x1804ae05c
1804adfe9 lea      rcx, [rip + 0x2ceeb20]
1804adff0 call     0x180309af0 ; 
1804adff5 lea      rcx, [rip + 0x2d190e4]
1804adffc call     0x180309af0 ; 
1804ae001 lea      rcx, [rip + 0x2d19208]
1804ae008 call     0x180309af0 ; 
1804ae00d lea      rcx, [rip + 0x2d0fbcc]
1804ae014 call     0x180309af0 ; 
1804ae019 lea      rcx, [rip + 0x2d186a0]
1804ae020 call     0x180309af0 ; 
1804ae025 lea      rcx, [rip + 0x2cc6fd4]
1804ae02c call     0x180309af0 ; 
1804ae031 lea      rcx, [rip + 0x2cc72b8]
1804ae038 call     0x180309af0 ; 
1804ae03d lea      rcx, [rip + 0x2cc7344]
1804ae044 call     0x180309af0 ; 
1804ae049 lea      rcx, [rip + 0x2cc73d0]
1804ae050 call     0x180309af0 ; 
1804ae055 mov      byte ptr [rip + 0x2e8f607], 1
1804ae05c mov      rcx, qword ptr [rdi + 0x20]
1804ae060 mov      qword ptr [rsp + 0x50], rbx
1804ae065 mov      qword ptr [rsp + 0x58], rbp
1804ae06a test     rcx, rcx
1804ae06d je       0x1804ae2e9
1804ae073 movups   xmm0, xmmword ptr [rsi]
1804ae076 mov      r8, qword ptr [rip + 0x2d19193]
1804ae07d lea      rdx, [rsp + 0x30]
1804ae082 movaps   xmmword ptr [rsp + 0x30], xmm0
1804ae087 call     0x18161cbb0 ; 
1804ae08c test     al, al
1804ae08e jne      0x1804ae0f5
1804ae090 mov      rcx, qword ptr [rdi + 0x10]
1804ae094 test     rcx, rcx
1804ae097 je       0x1804ae2e9
1804ae09d movups   xmm0, xmmword ptr [rsi]
1804ae0a0 mov      r8, qword ptr [rip + 0x2cc6f59]
1804ae0a7 lea      rdx, [rsp + 0x30]
1804ae0ac movaps   xmmword ptr [rsp + 0x30], xmm0
1804ae0b1 call     0x180fa5b50 ; 
1804ae0b6 mov      rcx, qword ptr [rdi + 0x20]
1804ae0ba test     rcx, rcx
1804ae0bd je       0x1804ae2e9
1804ae0c3 movups   xmm0, xmmword ptr [rsi]
1804ae0c6 mov      r8, qword ptr [rip + 0x2d19013]
1804ae0cd lea      rdx, [rsp + 0x30]
1804ae0d2 movaps   xmmword ptr [rsp + 0x30], xmm0
1804ae0d7 call     0x181619910 ; 
1804ae0dc mov      byte ptr [rdi + 0x28], 1
1804ae0e0 mov      rbp, qword ptr [rsp + 0x58]
1804ae0e5 mov      rbx, qword ptr [rsp + 0x50]
1804ae0ea mov      rsi, qword ptr [rsp + 0x60]
1804ae0ef add      rsp, 0x40
1804ae0f3 pop      rdi
1804ae0f4 ret      
1804ae0f5 cmp      byte ptr [rdi + 0x28], 0
1804ae0f9 je       0x1804ae105
1804ae0fb xor      edx, edx
1804ae0fd mov      rcx, rdi
1804ae100 call     0x1804af8d0 ; 9692:ifapp.Game.Data.IotaInventory.Sort
1804ae105 mov      rax, qword ptr [rdi + 0x10]
1804ae109 test     rax, rax
1804ae10c je       0x1804ae2e9
1804ae112 mov      rdx, qword ptr [rip + 0x2d0fac7]
1804ae119 mov      rbx, qword ptr [rax + 0x10]
1804ae11d mov      ebp, dword ptr [rax + 0x18]
1804ae120 cmp      dword ptr [rdx + 0xe4], 0
1804ae127 jne      0x1804ae138
1804ae129 mov      rcx, rdx
1804ae12c call     0x180309de0 ; 
1804ae131 mov      rdx, qword ptr [rip + 0x2d0faa8]
1804ae138 mov      rdx, qword ptr [rdx + 0xb8]
1804ae13f lea      r9, [rsp + 0x30]
1804ae144 mov      rax, qword ptr [rip + 0x2cee9c5]
1804ae14b mov      r8d, ebp
1804ae14e movups   xmm0, xmmword ptr [rsi]
1804ae151 mov      qword ptr [rsp + 0x28], rax
1804ae156 mov      rcx, rbx
1804ae159 mov      rax, qword ptr [rdx]
1804ae15c xor      edx, edx
1804ae15e mov      qword ptr [rsp + 0x20], rax
1804ae163 movaps   xmmword ptr [rsp + 0x30], xmm0
1804ae168 call     0x18079e660 ; 
1804ae16d mov      ebx, eax
1804ae16f test     eax, eax
1804ae171 jle      0x1804ae19b
1804ae173 mov      rcx, qword ptr [rdi + 0x10]
1804ae177 test     rcx, rcx
1804ae17a je       0x1804ae2e9
1804ae180 mov      r8, qword ptr [rip + 0x2cc7169]
1804ae187 lea      edx, [rbx - 1]
1804ae18a call     0x180fa5910 ; 
1804ae18f mov      ecx, dword ptr [rsi]
1804ae191 cmp      dword ptr [rax], ecx
1804ae193 jne      0x1804ae19b
1804ae195 dec      ebx
1804ae197 test     ebx, ebx
1804ae199 jg       0x1804ae173
1804ae19b mov      rax, qword ptr [rdi + 0x10]
1804ae19f test     rax, rax
1804ae1a2 je       0x1804ae2e9
1804ae1a8 nop      dword ptr [rax + rax]
1804ae1b0 cmp      ebx, dword ptr [rax + 0x18]
1804ae1b3 jge      0x1804ae261
1804ae1b9 mov      rcx, qword ptr [rdi + 0x10]
1804ae1bd test     rcx, rcx
1804ae1c0 je       0x1804ae2e9
1804ae1c6 mov      r8, qword ptr [rip + 0x2cc7123]
1804ae1cd mov      edx, ebx
1804ae1cf call     0x180fa5910 ; 
1804ae1d4 mov      ecx, dword ptr [rsi]
1804ae1d6 cmp      dword ptr [rax], ecx
1804ae1d8 jne      0x1804ae261
1804ae1de mov      rcx, qword ptr [rdi + 0x10]
1804ae1e2 test     rcx, rcx
1804ae1e5 je       0x1804ae2e9
1804ae1eb mov      r8, qword ptr [rip + 0x2cc70fe]
1804ae1f2 mov      edx, ebx
1804ae1f4 call     0x180fa5910 ; 
1804ae1f9 movzx    ecx, word ptr [rsi + 4]
1804ae1fd cmp      word ptr [rax + 4], cx
1804ae201 jne      0x1804ae24d
1804ae203 mov      rcx, qword ptr [rdi + 0x10]
1804ae207 test     rcx, rcx
1804ae20a je       0x1804ae2e9
1804ae210 mov      r8, qword ptr [rip + 0x2cc70d9]
1804ae217 mov      edx, ebx
1804ae219 call     0x180fa5910 ; 
1804ae21e movzx    ecx, word ptr [rsi + 6]
1804ae222 cmp      word ptr [rax + 6], cx
1804ae226 jne      0x1804ae24d
1804ae228 mov      rcx, qword ptr [rdi + 0x10]
1804ae22c test     rcx, rcx
1804ae22f je       0x1804ae2e9
1804ae235 mov      r8, qword ptr [rip + 0x2cc70b4]
1804ae23c mov      edx, ebx
1804ae23e call     0x180fa5910 ; 
1804ae243 movzx    ecx, word ptr [rsi + 8]
1804ae247 cmp      word ptr [rax + 8], cx
1804ae24b je       0x1804ae261
1804ae24d mov      rax, qword ptr [rdi + 0x10]
1804ae251 inc      ebx
1804ae253 test     rax, rax
1804ae256 je       0x1804ae2e9
1804ae25c jmp      0x1804ae1b0 ; 
1804ae261 mov      rcx, qword ptr [rdi + 0x10]
1804ae265 test     rcx, rcx
1804ae268 je       0x1804ae2e9
1804ae26a mov      r8, qword ptr [rip + 0x2cc707f]
1804ae271 mov      edx, ebx
1804ae273 call     0x180fa5910 ; 
1804ae278 mov      rcx, qword ptr [rip + 0x2d18441]
1804ae27f movzx    ebp, word ptr [rax + 0xc]
1804ae283 cmp      dword ptr [rcx + 0xe4], 0
1804ae28a jne      0x1804ae291
1804ae28c call     0x180309de0 ; 
1804ae291 cmp      byte ptr [rip + 0x2e8f0df], 0
1804ae298 jne      0x1804ae2b9
1804ae29a lea      rcx, [rip + 0x2ccb0cf]
1804ae2a1 call     0x180309af0 ; 
1804ae2a6 lea      rcx, [rip + 0x2d18413]
1804ae2ad call     0x180309af0 ; 
1804ae2b2 mov      byte ptr [rip + 0x2e8f0be], 1
1804ae2b9 movzx    edx, word ptr [rsi + 0xc]
1804ae2bd add      edx, ebp
1804ae2bf mov      rcx, qword ptr [rdi + 0x10]
1804ae2c3 mov      esi, 0xffff
1804ae2c8 cmp      edx, esi
1804ae2ca cmovbe   esi, edx
1804ae2cd test     rcx, rcx
1804ae2d0 je       0x1804ae2e9
1804ae2d2 mov      r8, qword ptr [rip + 0x2cc7017]
1804ae2d9 mov      edx, ebx
1804ae2db call     0x180fa5910 ; 
1804ae2e0 mov      word ptr [rax + 0xc], si
1804ae2e4 jmp      0x1804ae0e0 ; 
1804ae2e9 call     0x180309d40 ; 

FUNCTION 9692 IotaInventory Remove 0x1804aef40 1184
1804aef40 mov      qword ptr [rsp + 0x20], rbx
1804aef45 push     rsi
1804aef46 sub      rsp, 0x40
1804aef4a cmp      byte ptr [rip + 0x2e8e713], 0
1804aef51 mov      rsi, rdx
1804aef54 mov      rbx, rcx
1804aef57 jne      0x1804af00c
1804aef5d lea      rcx, [rip + 0x2cedbac]
1804aef64 call     0x180309af0 ; 
1804aef69 lea      rcx, [rip + 0x2ce7af8]
1804aef70 call     0x180309af0 ; 
1804aef75 lea      rcx, [rip + 0x2d18294]
1804aef7c call     0x180309af0 ; 
1804aef81 lea      rcx, [rip + 0x2d18320]
1804aef88 call     0x180309af0 ; 
1804aef8d lea      rcx, [rip + 0x2d03af4]
1804aef94 call     0x180309af0 ; 
1804aef99 lea      rcx, [rip + 0x2d0eba8]
1804aefa0 call     0x180309af0 ; 
1804aefa5 lea      rcx, [rip + 0x2d0ec34]
1804aefac call     0x180309af0 ; 
1804aefb1 lea      rcx, [rip + 0x2d17708]
1804aefb8 call     0x180309af0 ; 
1804aefbd lea      rcx, [rip + 0x2cc629c]
1804aefc4 call     0x180309af0 ; 
1804aefc9 lea      rcx, [rip + 0x2cc6320]
1804aefd0 call     0x180309af0 ; 
1804aefd5 lea      rcx, [rip + 0x2cc63ac]
1804aefdc call     0x180309af0 ; 
1804aefe1 lea      rcx, [rip + 0x2cc6438]
1804aefe8 call     0x180309af0 ; 
1804aefed lea      rcx, [rip + 0x2cea76c]
1804aeff4 call     0x180309af0 ; 
1804aeff9 lea      rcx, [rip + 0x2cfe908]
1804af000 call     0x180309af0 ; 
1804af005 mov      byte ptr [rip + 0x2e8e658], 1
1804af00c mov      rcx, qword ptr [rbx + 0x20]
1804af010 mov      qword ptr [rsp + 0x50], rbp
1804af015 mov      qword ptr [rsp + 0x58], rdi
1804af01a mov      qword ptr [rsp + 0x60], r14
1804af01f test     rcx, rcx
1804af022 je       0x1804af3d7
1804af028 movups   xmm0, xmmword ptr [rsi]
1804af02b mov      r8, qword ptr [rip + 0x2d181de]
1804af032 lea      rdx, [rsp + 0x30]
1804af037 movaps   xmmword ptr [rsp + 0x30], xmm0
1804af03c call     0x18161cbb0 ; 
1804af041 test     al, al
1804af043 jne      0x1804af0ea
1804af049 mov      rcx, qword ptr [rip + 0x2ce7a18]
1804af050 cmp      dword ptr [rcx + 0xe4], 0
1804af057 jne      0x1804af05e
1804af059 call     0x180309de0 ; 
1804af05e cmp      byte ptr [rip + 0x2e8decc], 0
1804af065 jne      0x1804af07a
1804af067 lea      rcx, [rip + 0x2ce79fa]
1804af06e call     0x180309af0 ; 
1804af073 mov      byte ptr [rip + 0x2e8deb7], 1
1804af07a mov      rcx, qword ptr [rip + 0x2ce79e7]
1804af081 cmp      dword ptr [rcx + 0xe4], 0
1804af088 jne      0x1804af08f
1804af08a call     0x180309de0 ; 
1804af08f mov      rax, qword ptr [rip + 0x2ce79d2]
1804af096 mov      rcx, qword ptr [rax + 0xb8]
1804af09d mov      r8, qword ptr [rcx + 8]
1804af0a1 test     r8, r8
1804af0a4 je       0x1804af3d7
1804af0aa mov      rax, qword ptr [rip + 0x2cea6af]
1804af0b1 mov      ecx, 8
1804af0b6 mov      r9, qword ptr [rip + 0x2cfe84b]
1804af0bd mov      rdx, qword ptr [rip + 0x2d039c4]
1804af0c4 mov      qword ptr [rsp + 0x20], rax
1804af0c9 call     0x1800042b0 ; 
1804af0ce xor      eax, eax
1804af0d0 mov      r14, qword ptr [rsp + 0x60]
1804af0d5 mov      rdi, qword ptr [rsp + 0x58]
1804af0da mov      rbp, qword ptr [rsp + 0x50]
1804af0df mov      rbx, qword ptr [rsp + 0x68]
1804af0e4 add      rsp, 0x40
1804af0e8 pop      rsi
1804af0e9 ret      
1804af0ea cmp      byte ptr [rbx + 0x28], 0
1804af0ee je       0x1804af0fa
1804af0f0 xor      edx, edx
1804af0f2 mov      rcx, rbx
1804af0f5 call     0x1804af8d0 ; 9692:ifapp.Game.Data.IotaInventory.Sort
1804af0fa mov      rax, qword ptr [rbx + 0x10]
1804af0fe test     rax, rax
1804af101 je       0x1804af3d7
1804af107 mov      rdx, qword ptr [rip + 0x2d0ead2]
1804af10e mov      rdi, qword ptr [rax + 0x10]
1804af112 mov      ebp, dword ptr [rax + 0x18]
1804af115 cmp      dword ptr [rdx + 0xe4], 0
1804af11c jne      0x1804af12d
1804af11e mov      rcx, rdx
1804af121 call     0x180309de0 ; 
1804af126 mov      rdx, qword ptr [rip + 0x2d0eab3]
1804af12d mov      rdx, qword ptr [rdx + 0xb8]
1804af134 lea      r9, [rsp + 0x30]
1804af139 mov      rax, qword ptr [rip + 0x2ced9d0]
1804af140 mov      r8d, ebp
1804af143 movups   xmm0, xmmword ptr [rsi]
1804af146 mov      qword ptr [rsp + 0x28], rax
1804af14b mov      rcx, rdi
1804af14e mov      rax, qword ptr [rdx]
1804af151 xor      edx, edx
1804af153 mov      qword ptr [rsp + 0x20], rax
1804af158 movaps   xmmword ptr [rsp + 0x30], xmm0
1804af15d call     0x18079e660 ; 
1804af162 mov      edi, eax
1804af164 test     eax, eax
1804af166 jle      0x1804af198
1804af168 nop      dword ptr [rax + rax]
1804af170 mov      rcx, qword ptr [rbx + 0x10]
1804af174 test     rcx, rcx
1804af177 je       0x1804af3d7
1804af17d mov      r8, qword ptr [rip + 0x2cc616c]
1804af184 lea      edx, [rdi - 1]
1804af187 call     0x180fa5910 ; 
1804af18c mov      ecx, dword ptr [rsi]
1804af18e cmp      dword ptr [rax], ecx
1804af190 jne      0x1804af198
1804af192 dec      edi
1804af194 test     edi, edi
1804af196 jg       0x1804af170
1804af198 mov      rax, qword ptr [rbx + 0x10]
1804af19c xor      ecx, ecx
1804af19e test     edi, edi
1804af1a0 cmovns   ecx, edi
1804af1a3 test     rax, rax
1804af1a6 je       0x1804af3d7
1804af1ac mov      edi, ecx
1804af1ae mov      ebp, 0x3ff
1804af1b3 cmp      ecx, dword ptr [rax + 0x18]
1804af1b6 jge      0x1804af0ce
1804af1bc mov      rcx, qword ptr [rbx + 0x10]
1804af1c0 test     rcx, rcx
1804af1c3 je       0x1804af3d7
1804af1c9 mov      r8, qword ptr [rip + 0x2cc6120]
1804af1d0 mov      edx, edi
1804af1d2 call     0x180fa5910 ; 
1804af1d7 mov      ecx, dword ptr [rsi]
1804af1d9 cmp      dword ptr [rax], ecx
1804af1db jne      0x1804af0ce
1804af1e1 mov      rcx, qword ptr [rbx + 0x10]
1804af1e5 test     rcx, rcx
1804af1e8 je       0x1804af3d7
1804af1ee mov      r8, qword ptr [rip + 0x2cc60fb]
1804af1f5 mov      edx, edi
1804af1f7 call     0x180fa5910 ; 
1804af1fc mov      rcx, qword ptr [rip + 0x2d0e945]
1804af203 mov      r14, rax
1804af206 cmp      dword ptr [rcx + 0xe4], 0
1804af20d jne      0x1804af214
1804af20f call     0x180309de0 ; 
1804af214 mov      ecx, dword ptr [r14]
1804af217 mov      eax, dword ptr [rsi]
1804af219 sar      ecx, 0xc
1804af21c sar      eax, 0xc
1804af21f and      cl, 0x3f
1804af222 and      al, 0x3f
1804af224 cmp      cl, al
1804af226 jne      0x1804af2e4
1804af22c mov      rcx, qword ptr [rbx + 0x10]
1804af230 test     rcx, rcx
1804af233 je       0x1804af3d7
1804af239 mov      r8, qword ptr [rip + 0x2cc60b0]
1804af240 mov      edx, edi
1804af242 call     0x180fa5910 ; 
1804af247 mov      rcx, qword ptr [rip + 0x2d0e8fa]
1804af24e mov      r14, rax
1804af251 cmp      dword ptr [rcx + 0xe4], 0
1804af258 jne      0x1804af25f
1804af25a call     0x180309de0 ; 
1804af25f mov      ecx, dword ptr [rsi]
1804af261 mov      eax, dword ptr [r14]
1804af264 sar      ecx, 2
1804af267 sar      eax, 2
1804af26a and      cx, bp
1804af26d and      ax, bp
1804af270 cmp      ax, cx
1804af273 jne      0x1804af2e4
1804af275 mov      rcx, qword ptr [rbx + 0x10]
1804af279 test     rcx, rcx
1804af27c je       0x1804af3d7
1804af282 mov      r8, qword ptr [rip + 0x2cc6067]
1804af289 mov      edx, edi
1804af28b call     0x180fa5910 ; 
1804af290 movzx    ecx, word ptr [rsi + 4]
1804af294 cmp      word ptr [rax + 4], cx
1804af298 jne      0x1804af2e4
1804af29a mov      rcx, qword ptr [rbx + 0x10]
1804af29e test     rcx, rcx
1804af2a1 je       0x1804af3d7
1804af2a7 mov      r8, qword ptr [rip + 0x2cc6042]
1804af2ae mov      edx, edi
1804af2b0 call     0x180fa5910 ; 
1804af2b5 movzx    ecx, word ptr [rsi + 6]
1804af2b9 cmp      word ptr [rax + 6], cx
1804af2bd jne      0x1804af2e4
1804af2bf mov      rcx, qword ptr [rbx + 0x10]
1804af2c3 test     rcx, rcx
1804af2c6 je       0x1804af3d7
1804af2cc mov      r8, qword ptr [rip + 0x2cc601d]
1804af2d3 mov      edx, edi
1804af2d5 call     0x180fa5910 ; 
1804af2da movzx    ecx, word ptr [rsi + 8]
1804af2de cmp      word ptr [rax + 8], cx
1804af2e2 je       0x1804af2fa
1804af2e4 mov      rax, qword ptr [rbx + 0x10]
1804af2e8 inc      edi
1804af2ea mov      ecx, edi
1804af2ec test     rax, rax
1804af2ef je       0x1804af3d7
1804af2f5 jmp      0x1804af1b3 ; 
1804af2fa mov      rcx, qword ptr [rbx + 0x10]
1804af2fe test     rcx, rcx
1804af301 je       0x1804af3d7
1804af307 mov      r8, qword ptr [rip + 0x2cc5fe2]
1804af30e mov      edx, edi
1804af310 call     0x180fa5910 ; 
1804af315 mov      rcx, qword ptr [rip + 0x2d173a4]
1804af31c movzx    ebp, word ptr [rax + 0xc]
1804af320 cmp      dword ptr [rcx + 0xe4], 0
1804af327 jne      0x1804af32e
1804af329 call     0x180309de0 ; 
1804af32e movzx    eax, word ptr [rsi + 0xc]
1804af332 sub      ebp, eax
1804af334 cmp      byte ptr [rip + 0x2e8e03c], 0
1804af33b jne      0x1804af35c
1804af33d lea      rcx, [rip + 0x2cca02c]
1804af344 call     0x180309af0 ; 
1804af349 lea      rcx, [rip + 0x2d17370]
1804af350 call     0x180309af0 ; 
1804af355 mov      byte ptr [rip + 0x2e8e01b], 1
1804af35c mov      rcx, qword ptr [rbx + 0x10]
1804af360 test     ebp, ebp
1804af362 js       0x1804af395
1804af364 cmp      ebp, 0xffff
1804af36a jg       0x1804af372
1804af36c test     ebp, ebp
1804af36e je       0x1804af397
1804af370 jmp      0x1804af377 ; 
1804af372 mov      ebp, 0xffff
1804af377 test     rcx, rcx
1804af37a je       0x1804af3d7
1804af37c mov      r8, qword ptr [rip + 0x2cc5f6d]
1804af383 mov      edx, edi
1804af385 call     0x180fa5910 ; 
1804af38a mov      word ptr [rax + 0xc], bp
1804af38e mov      eax, ebp
1804af390 jmp      0x1804af0d0 ; 
1804af395 xor      ebp, ebp
1804af397 test     rcx, rcx
1804af39a je       0x1804af3d7
1804af39c mov      r8, qword ptr [rip + 0x2cc5ebd]
1804af3a3 mov      edx, edi
1804af3a5 call     0x180fa4a00 ; 
1804af3aa mov      rcx, qword ptr [rbx + 0x20]
1804af3ae test     rcx, rcx
1804af3b1 je       0x1804af3d7
1804af3b3 movups   xmm0, xmmword ptr [rsi]
1804af3b6 mov      r8, qword ptr [rip + 0x2d17eeb]
1804af3bd lea      rdx, [rsp + 0x30]
1804af3c2 movaps   xmmword ptr [rsp + 0x30], xmm0
1804af3c7 call     0x18162a480 ; 
1804af3cc mov      byte ptr [rbx + 0x28], 1
1804af3d0 mov      eax, ebp
1804af3d2 jmp      0x1804af0d0 ; 
1804af3d7 call     0x180309d40 ; 

FUNCTION 9692 IotaInventory GetHighestBaseIDPotency 0x1804ae850 288
1804ae850 mov      qword ptr [rsp + 0x18], rbx
1804ae855 push     rbp
1804ae856 push     rsi
1804ae857 push     r14
1804ae859 sub      rsp, 0x20
1804ae85d cmp      byte ptr [rip + 0x2e8ee01], 0
1804ae864 mov      rbp, rdx
1804ae867 mov      r14, rcx
1804ae86a jne      0x1804ae897
1804ae86c lea      rcx, [rip + 0x2d0f2d5]
1804ae873 call     0x180309af0 ; 
1804ae878 lea      rcx, [rip + 0x2cc6a71]
1804ae87f call     0x180309af0 ; 
1804ae884 lea      rcx, [rip + 0x2cc6afd]
1804ae88b call     0x180309af0 ; 
1804ae890 mov      byte ptr [rip + 0x2e8edce], 1
1804ae897 xor      esi, esi
1804ae899 xor      ebx, ebx
1804ae89b xor      eax, eax
1804ae89d test     rbp, rbp
1804ae8a0 je       0x1804ae95d
1804ae8a6 mov      qword ptr [rsp + 0x48], r15
1804ae8ab mov      r15d, 0x3ff
1804ae8b1 mov      qword ptr [rsp + 0x40], rdi
1804ae8b6 cmp      eax, dword ptr [rbp + 0x18]
1804ae8b9 jge      0x1804ae942
1804ae8bf mov      r8, qword ptr [rip + 0x2cc6a2a]
1804ae8c6 mov      edx, ebx
1804ae8c8 mov      rcx, rbp
1804ae8cb call     0x180fa5910 ; 
1804ae8d0 mov      rcx, qword ptr [rip + 0x2d0f271]
1804ae8d7 mov      rdi, rax
1804ae8da cmp      dword ptr [rcx + 0xe4], 0
1804ae8e1 jne      0x1804ae8ef
1804ae8e3 call     0x180309de0 ; 
1804ae8e8 mov      rcx, qword ptr [rip + 0x2d0f259]
1804ae8ef mov      eax, dword ptr [rdi]
1804ae8f1 sar      eax, 0xc
1804ae8f4 and      ax, 0x3f
1804ae8f8 cmp      ax, word ptr [r14 + 8]
1804ae8fd jne      0x1804ae939
1804ae8ff cmp      dword ptr [rcx + 0xe4], 0
1804ae906 jne      0x1804ae914
1804ae908 call     0x180309de0 ; 
1804ae90d mov      rcx, qword ptr [rip + 0x2d0f234]
1804ae914 mov      eax, dword ptr [rdi]
1804ae916 sar      eax, 2
1804ae919 and      ax, r15w
1804ae91d cmp      ax, si
1804ae920 jbe      0x1804ae939
1804ae922 cmp      dword ptr [rcx + 0xe4], 0
1804ae929 jne      0x1804ae930
1804ae92b call     0x180309de0 ; 
1804ae930 mov      esi, dword ptr [rdi]
1804ae932 sar      esi, 2
1804ae935 and      si, r15w
1804ae939 inc      ebx
1804ae93b mov      eax, ebx
1804ae93d jmp      0x1804ae8b6 ; 
1804ae942 mov      r15, qword ptr [rsp + 0x48]
1804ae947 movzx    eax, si
1804ae94a mov      rdi, qword ptr [rsp + 0x40]
1804ae94f mov      rbx, qword ptr [rsp + 0x50]
1804ae954 add      rsp, 0x20
1804ae958 pop      r14
1804ae95a pop      rsi
1804ae95b pop      rbp
1804ae95c ret      
1804ae95d call     0x180309d40 ; 

FUNCTION 9692 IotaInventory GetHighestBaseIDPotencyFiltered 0x1804ae6f0 352
1804ae6f0 mov      qword ptr [rsp + 0x20], r9
1804ae6f5 push     rbp
1804ae6f6 push     rsi
1804ae6f7 push     rdi
1804ae6f8 push     r14
1804ae6fa push     r15
1804ae6fc sub      rsp, 0x20
1804ae700 cmp      byte ptr [rip + 0x2e8ef5f], 0
1804ae707 mov      r15, r8
1804ae70a mov      rbp, rdx
1804ae70d mov      r14, rcx
1804ae710 jne      0x1804ae749
1804ae712 lea      rcx, [rip + 0x2d0f2ff]
1804ae719 call     0x180309af0 ; 
1804ae71e lea      rcx, [rip + 0x2d0f423]
1804ae725 call     0x180309af0 ; 
1804ae72a lea      rcx, [rip + 0x2cc6bbf]
1804ae731 call     0x180309af0 ; 
1804ae736 lea      rcx, [rip + 0x2cc6c4b]
1804ae73d call     0x180309af0 ; 
1804ae742 mov      byte ptr [rip + 0x2e8ef1d], 1
1804ae749 xor      esi, esi
1804ae74b xor      edi, edi
1804ae74d xor      eax, eax
1804ae74f test     rbp, rbp
1804ae752 je       0x1804ae841
1804ae758 mov      qword ptr [rsp + 0x58], r12
1804ae75d mov      r12d, 0x3ff
1804ae763 mov      qword ptr [rsp + 0x50], rbx
1804ae768 nop      dword ptr [rax + rax]
1804ae770 cmp      eax, dword ptr [rbp + 0x18]
1804ae773 jge      0x1804ae828
1804ae779 mov      r8, qword ptr [rip + 0x2cc6b70]
1804ae780 mov      edx, edi
1804ae782 mov      rcx, rbp
1804ae785 call     0x180fa5910 ; 
1804ae78a mov      rcx, qword ptr [rip + 0x2d0f3b7]
1804ae791 mov      rbx, rax
1804ae794 cmp      dword ptr [rcx + 0xe4], 0
1804ae79b jne      0x1804ae7a9
1804ae79d call     0x180309de0 ; 
1804ae7a2 mov      rcx, qword ptr [rip + 0x2d0f39f]
1804ae7a9 mov      eax, dword ptr [rbx]
1804ae7ab sar      eax, 0xc
1804ae7ae and      ax, 0x3f
1804ae7b2 cmp      ax, word ptr [r14 + 8]
1804ae7b7 jne      0x1804ae81f
1804ae7b9 cmp      dword ptr [rcx + 0xe4], 0
1804ae7c0 jne      0x1804ae7c7
1804ae7c2 call     0x180309de0 ; 
1804ae7c7 mov      eax, dword ptr [rbx]
1804ae7c9 sar      eax, 2
1804ae7cc and      ax, r12w
1804ae7d0 cmp      ax, si
1804ae7d3 jbe      0x1804ae81f
1804ae7d5 mov      rcx, qword ptr [rip + 0x2d0f23c]
1804ae7dc cmp      dword ptr [rcx + 0xe4], 0
1804ae7e3 jne      0x1804ae7ea
1804ae7e5 call     0x180309de0 ; 
1804ae7ea xor      r9d, r9d
1804ae7ed lea      rcx, [rsp + 0x68]
1804ae7f2 mov      r8, r15
1804ae7f5 mov      rdx, rbx
1804ae7f8 call     0x1804acc90 ; 9667:ifapp.Game.Data.IotaFilterParameters._NnA
1804ae7fd test     al, al
1804ae7ff je       0x1804ae81f
1804ae801 mov      rcx, qword ptr [rip + 0x2d0f340]
1804ae808 cmp      dword ptr [rcx + 0xe4], 0
1804ae80f jne      0x1804ae816
1804ae811 call     0x180309de0 ; 
1804ae816 mov      esi, dword ptr [rbx]
1804ae818 sar      esi, 2
1804ae81b and      si, r12w
1804ae81f inc      edi
1804ae821 mov      eax, edi
1804ae823 jmp      0x1804ae770 ; 
1804ae828 mov      r12, qword ptr [rsp + 0x58]
1804ae82d movzx    eax, si
1804ae830 mov      rbx, qword ptr [rsp + 0x50]
1804ae835 add      rsp, 0x20
1804ae839 pop      r15
1804ae83b pop      r14
1804ae83d pop      rdi
1804ae83e pop      rsi
1804ae83f pop      rbp
1804ae840 ret      
1804ae841 call     0x180309d40 ; 

FUNCTION 9692 IotaInventory GetIotaDetailHasTrait 0x1804aea60 240
1804aea60 mov      qword ptr [rsp + 0x10], rbx
1804aea65 mov      qword ptr [rsp + 0x18], rbp
1804aea6a push     rsi
1804aea6b sub      rsp, 0x20
1804aea6f cmp      byte ptr [rip + 0x2e8ebf1], 0
1804aea76 mov      rsi, rdx
1804aea79 mov      rbp, rcx
1804aea7c jne      0x1804aeaa9
1804aea7e lea      rcx, [rip + 0x2d0f0c3]
1804aea85 call     0x180309af0 ; 
1804aea8a lea      rcx, [rip + 0x2cc685f]
1804aea91 call     0x180309af0 ; 
1804aea96 lea      rcx, [rip + 0x2cc68eb]
1804aea9d call     0x180309af0 ; 
1804aeaa2 mov      byte ptr [rip + 0x2e8ebbe], 1
1804aeaa9 xor      ebx, ebx
1804aeaab xor      eax, eax
1804aeaad test     rsi, rsi
1804aeab0 je       0x1804aeb44
1804aeab6 mov      qword ptr [rsp + 0x30], rdi
1804aeabb nop      dword ptr [rax + rax]
1804aeac0 cmp      eax, dword ptr [rsi + 0x18]
1804aeac3 jge      0x1804aeb40
1804aeac5 mov      r8, qword ptr [rip + 0x2cc6824]
1804aeacc mov      edx, ebx
1804aeace mov      rcx, rsi
1804aead1 call     0x180fa5910 ; 
1804aead6 mov      rcx, qword ptr [rip + 0x2d0f06b]
1804aeadd mov      rdi, rax
1804aeae0 cmp      dword ptr [rcx + 0xe4], 0
1804aeae7 jne      0x1804aeaf5
1804aeae9 call     0x180309de0 ; 
1804aeaee mov      rcx, qword ptr [rip + 0x2d0f053]
1804aeaf5 mov      edx, dword ptr [rdi]
1804aeaf7 sar      edx, 0xc
1804aeafa and      dx, 0x3f
1804aeafe cmp      dx, word ptr [rbp + 8]
1804aeb02 jne      0x1804aeb23
1804aeb04 cmp      dword ptr [rcx + 0xe4], 0
1804aeb0b jne      0x1804aeb12
1804aeb0d call     0x180309de0 ; 
1804aeb12 movzx    ecx, word ptr [rdi]
1804aeb15 mov      eax, 3
1804aeb1a and      cx, 3
1804aeb1e cmp      ax, cx
1804aeb21 jne      0x1804aeb29
1804aeb23 inc      ebx
1804aeb25 mov      eax, ebx
1804aeb27 jmp      0x1804aeac0 ; 
1804aeb29 mov      al, 1
1804aeb2b mov      rdi, qword ptr [rsp + 0x30]
1804aeb30 mov      rbx, qword ptr [rsp + 0x38]
1804aeb35 mov      rbp, qword ptr [rsp + 0x40]
1804aeb3a add      rsp, 0x20
1804aeb3e pop      rsi
1804aeb3f ret      
1804aeb40 xor      al, al
1804aeb42 jmp      0x1804aeb2b ; 
1804aeb44 call     0x180309d40 ; 

FUNCTION 9692 IotaInventory GetIotaDetailHasNoTrait 0x1804ae970 240
1804ae970 mov      qword ptr [rsp + 0x10], rbx
1804ae975 mov      qword ptr [rsp + 0x18], rbp
1804ae97a push     rsi
1804ae97b sub      rsp, 0x20
1804ae97f cmp      byte ptr [rip + 0x2e8ece2], 0
1804ae986 mov      rsi, rdx
1804ae989 mov      rbp, rcx
1804ae98c jne      0x1804ae9b9
1804ae98e lea      rcx, [rip + 0x2d0f1b3]
1804ae995 call     0x180309af0 ; 
1804ae99a lea      rcx, [rip + 0x2cc694f]
1804ae9a1 call     0x180309af0 ; 
1804ae9a6 lea      rcx, [rip + 0x2cc69db]
1804ae9ad call     0x180309af0 ; 
1804ae9b2 mov      byte ptr [rip + 0x2e8ecaf], 1
1804ae9b9 xor      ebx, ebx
1804ae9bb xor      eax, eax
1804ae9bd test     rsi, rsi
1804ae9c0 je       0x1804aea54
1804ae9c6 mov      qword ptr [rsp + 0x30], rdi
1804ae9cb nop      dword ptr [rax + rax]
1804ae9d0 cmp      eax, dword ptr [rsi + 0x18]
1804ae9d3 jge      0x1804aea50
1804ae9d5 mov      r8, qword ptr [rip + 0x2cc6914]
1804ae9dc mov      edx, ebx
1804ae9de mov      rcx, rsi
1804ae9e1 call     0x180fa5910 ; 
1804ae9e6 mov      rcx, qword ptr [rip + 0x2d0f15b]
1804ae9ed mov      rdi, rax
1804ae9f0 cmp      dword ptr [rcx + 0xe4], 0
1804ae9f7 jne      0x1804aea05
1804ae9f9 call     0x180309de0 ; 
1804ae9fe mov      rcx, qword ptr [rip + 0x2d0f143]
1804aea05 mov      edx, dword ptr [rdi]
1804aea07 sar      edx, 0xc
1804aea0a and      dx, 0x3f
1804aea0e cmp      dx, word ptr [rbp + 8]
1804aea12 jne      0x1804aea33
1804aea14 cmp      dword ptr [rcx + 0xe4], 0
1804aea1b jne      0x1804aea22
1804aea1d call     0x180309de0 ; 
1804aea22 movzx    ecx, word ptr [rdi]
1804aea25 mov      eax, 3
1804aea2a and      cx, 3
1804aea2e cmp      ax, cx
1804aea31 je       0x1804aea39
1804aea33 inc      ebx
1804aea35 mov      eax, ebx
1804aea37 jmp      0x1804ae9d0 ; 
1804aea39 mov      al, 1
1804aea3b mov      rdi, qword ptr [rsp + 0x30]
1804aea40 mov      rbx, qword ptr [rsp + 0x38]
1804aea45 mov      rbp, qword ptr [rsp + 0x40]
1804aea4a add      rsp, 0x20
1804aea4e pop      rsi
1804aea4f ret      
1804aea50 xor      al, al
1804aea52 jmp      0x1804aea3b ; 
1804aea54 call     0x180309d40 ; 

FUNCTION 9692 IotaInventory Sort 0x1804af820 176
1804af820 mov      qword ptr [rsp + 8], rbx
1804af825 mov      qword ptr [rsp + 0x10], rsi
1804af82a push     rdi
1804af82b sub      rsp, 0x30
1804af82f cmp      byte ptr [rip + 0x2e8de33], 0
1804af836 mov      edi, edx
1804af838 movsxd   rbx, r8d
1804af83b mov      rsi, rcx
1804af83e jne      0x1804af85f
1804af840 lea      rcx, [rip + 0x2cf2151]
1804af847 call     0x180309af0 ; 
1804af84c lea      rcx, [rip + 0x2d0e38d]
1804af853 call     0x180309af0 ; 
1804af858 mov      byte ptr [rip + 0x2e8de0a], 1
1804af85f mov      rax, qword ptr [rip + 0x2d0e37a]
1804af866 cmp      dword ptr [rax + 0xe4], 0
1804af86d jne      0x1804af87e
1804af86f mov      rcx, rax
1804af872 call     0x180309de0 ; 
1804af877 mov      rax, qword ptr [rip + 0x2d0e362]
1804af87e mov      rax, qword ptr [rax + 0xb8]
1804af885 mov      rcx, qword ptr [rax + 8]
1804af889 test     rcx, rcx
1804af88c je       0x1804af8c1
1804af88e cmp      ebx, dword ptr [rcx + 0x18]
1804af891 jae      0x1804af8c7
1804af893 mov      rax, qword ptr [rip + 0x2cf20fe]
1804af89a mov      r8d, edi
1804af89d mov      r9, qword ptr [rcx + rbx*8 + 0x20]
1804af8a2 xor      edx, edx
1804af8a4 mov      rcx, rsi
1804af8a7 mov      qword ptr [rsp + 0x20], rax
1804af8ac call     0x1807d2d20 ; 
1804af8b1 mov      rbx, qword ptr [rsp + 0x40]
1804af8b6 mov      rsi, qword ptr [rsp + 0x48]
1804af8bb add      rsp, 0x30
1804af8bf pop      rdi
1804af8c0 ret      
1804af8c1 call     0x180309d40 ; 
1804af8c7 call     0x180309d30 ; 

FUNCTION 9692 IotaInventory Sort 0x1804af510 784
1804af510 push     rbx
1804af512 push     rbp
1804af513 push     rsi
1804af514 push     rdi
1804af515 sub      rsp, 0x38
1804af519 cmp      byte ptr [rip + 0x2e8e14a], 0
1804af520 mov      rdi, r9
1804af523 movsxd   rbx, r8d
1804af526 mov      esi, edx
1804af528 mov      rbp, rcx
1804af52b jne      0x1804af564
1804af52d lea      rcx, [rip + 0x2cf23c4]
1804af534 call     0x180309af0 ; 
1804af539 lea      rcx, [rip + 0x2d0e6a0]
1804af540 call     0x180309af0 ; 
1804af545 lea      rcx, [rip + 0x2cbdb6c]
1804af54c call     0x180309af0 ; 
1804af551 lea      rcx, [rip + 0x2cbdc90]
1804af558 call     0x180309af0 ; 
1804af55d mov      byte ptr [rip + 0x2e8e106], 1
1804af564 cmp      ebx, 6
1804af567 jne      0x1804af5f0
1804af56d mov      rax, qword ptr [rip + 0x2d0e66c]
1804af574 cmp      dword ptr [rax + 0xe4], 0
1804af57b jne      0x1804af58c
1804af57d mov      rcx, rax
1804af580 call     0x180309de0 ; 
1804af585 mov      rax, qword ptr [rip + 0x2d0e654]
1804af58c mov      rax, qword ptr [rax + 0xb8]
1804af593 mov      r8, qword ptr [rax + 0x18]
1804af597 test     r8, r8
1804af59a je       0x1804af7e1
1804af5a0 cmp      dword ptr [r8 + 0x18], 6
1804af5a5 jbe      0x1804af80b
1804af5ab mov      r8, qword ptr [r8 + 0x50]
1804af5af test     r8, r8
1804af5b2 je       0x1804af7e1
1804af5b8 mov      rdx, qword ptr [rip + 0x2cbdaf9]
1804af5bf mov      r9, qword ptr [r8]
1804af5c2 movzx    eax, byte ptr [rdx + 0x130]
1804af5c9 cmp      byte ptr [r9 + 0x130], al
1804af5d0 jb       0x1804af7e7
1804af5d6 movzx    ecx, al
1804af5d9 mov      rax, qword ptr [r9 + 0xc8]
1804af5e0 cmp      qword ptr [rax + rcx*8 - 8], rdx
1804af5e5 jne      0x1804af7e7
1804af5eb jmp      0x1804af677 ; 
1804af5f0 cmp      ebx, 7
1804af5f3 jne      0x1804af67b
1804af5f9 mov      rax, qword ptr [rip + 0x2d0e5e0]
1804af600 cmp      dword ptr [rax + 0xe4], 0
1804af607 jne      0x1804af618
1804af609 mov      rcx, rax
1804af60c call     0x180309de0 ; 
1804af611 mov      rax, qword ptr [rip + 0x2d0e5c8]
1804af618 mov      rax, qword ptr [rax + 0xb8]
1804af61f mov      r8, qword ptr [rax + 0x18]
1804af623 test     r8, r8
1804af626 je       0x1804af7e1
1804af62c cmp      dword ptr [r8 + 0x18], 7
1804af631 jbe      0x1804af80b
1804af637 mov      r8, qword ptr [r8 + 0x58]
1804af63b test     r8, r8
1804af63e je       0x1804af7e1
1804af644 mov      rdx, qword ptr [rip + 0x2cbdb9d]
1804af64b mov      r9, qword ptr [r8]
1804af64e movzx    eax, byte ptr [rdx + 0x130]
1804af655 cmp      byte ptr [r9 + 0x130], al
1804af65c jb       0x1804af7f0
1804af662 movzx    ecx, al
1804af665 mov      rax, qword ptr [r9 + 0xc8]
1804af66c cmp      qword ptr [rax + rcx*8 - 8], rdx
1804af671 jne      0x1804af7f0
1804af677 mov      qword ptr [r8 + 0x10], rdi
1804af67b mov      rax, qword ptr [rip + 0x2d0e55e]
1804af682 cmp      dword ptr [rax + 0xe4], 0
1804af689 jne      0x1804af69a
1804af68b mov      rcx, rax
1804af68e call     0x180309de0 ; 
1804af693 mov      rax, qword ptr [rip + 0x2d0e546]
1804af69a mov      rax, qword ptr [rax + 0xb8]
1804af6a1 mov      rcx, qword ptr [rax + 0x18]
1804af6a5 test     rcx, rcx
1804af6a8 je       0x1804af7e1
1804af6ae cmp      ebx, dword ptr [rcx + 0x18]
1804af6b1 jae      0x1804af80b
1804af6b7 mov      rax, qword ptr [rip + 0x2cf223a]
1804af6be mov      r8d, esi
1804af6c1 mov      r9, qword ptr [rcx + rbx*8 + 0x20]
1804af6c6 xor      edx, edx
1804af6c8 mov      rcx, rbp
1804af6cb mov      qword ptr [rsp + 0x20], rax
1804af6d0 call     0x1807d27e0 ; 
1804af6d5 cmp      ebx, 6
1804af6d8 jne      0x1804af76d
1804af6de mov      rax, qword ptr [rip + 0x2d0e4fb]
1804af6e5 cmp      dword ptr [rax + 0xe4], 0
1804af6ec jne      0x1804af6fd
1804af6ee mov      rcx, rax
1804af6f1 call     0x180309de0 ; 
1804af6f6 mov      rax, qword ptr [rip + 0x2d0e4e3]
1804af6fd mov      rax, qword ptr [rax + 0xb8]
1804af704 mov      r8, qword ptr [rax + 0x18]
1804af708 test     r8, r8
1804af70b je       0x1804af7e1
1804af711 cmp      dword ptr [r8 + 0x18], 6
1804af716 jbe      0x1804af80b
1804af71c mov      r8, qword ptr [r8 + 0x50]
1804af720 test     r8, r8
1804af723 je       0x1804af7e1
1804af729 mov      rdx, qword ptr [rip + 0x2cbd988]
1804af730 mov      r9, qword ptr [r8]
1804af733 movzx    eax, byte ptr [rdx + 0x130]
1804af73a cmp      byte ptr [r9 + 0x130], al
1804af741 jb       0x1804af7f9
1804af747 movzx    ecx, al
1804af74a mov      rax, qword ptr [r9 + 0xc8]
1804af751 cmp      qword ptr [rax + rcx*8 - 8], rdx
1804af756 jne      0x1804af7f9
1804af75c mov      qword ptr [r8 + 0x10], 0
1804af764 add      rsp, 0x38
1804af768 pop      rdi
1804af769 pop      rsi
1804af76a pop      rbp
1804af76b pop      rbx
1804af76c ret      
1804af76d cmp      ebx, 7
1804af770 jne      0x1804af764
1804af772 mov      rax, qword ptr [rip + 0x2d0e467]
1804af779 cmp      dword ptr [rax + 0xe4], 0
1804af780 jne      0x1804af791
1804af782 mov      rcx, rax
1804af785 call     0x180309de0 ; 
1804af78a mov      rax, qword ptr [rip + 0x2d0e44f]
1804af791 mov      rax, qword ptr [rax + 0xb8]
1804af798 mov      r8, qword ptr [rax + 0x18]
1804af79c test     r8, r8
1804af79f je       0x1804af7e1
1804af7a1 cmp      dword ptr [r8 + 0x18], 7
1804af7a6 jbe      0x1804af80b
1804af7a8 mov      r8, qword ptr [r8 + 0x58]
1804af7ac test     r8, r8
1804af7af je       0x1804af7e1
1804af7b1 mov      rdx, qword ptr [rip + 0x2cbda30]
1804af7b8 mov      r9, qword ptr [r8]
1804af7bb movzx    eax, byte ptr [rdx + 0x130]
1804af7c2 cmp      byte ptr [r9 + 0x130], al
1804af7c9 jb       0x1804af802
1804af7cb movzx    ecx, al
1804af7ce mov      rax, qword ptr [r9 + 0xc8]
1804af7d5 cmp      qword ptr [rax + rcx*8 - 8], rdx
1804af7da jne      0x1804af802
1804af7dc jmp      0x1804af75c ; 
1804af7e1 call     0x180309d40 ; 
1804af7e7 mov      rcx, r8
1804af7ea call     0x180308eb0 ; 
1804af7f0 mov      rcx, r8
1804af7f3 call     0x180308eb0 ; 
1804af7f9 mov      rcx, r8
1804af7fc call     0x180308eb0 ; 
1804af802 mov      rcx, r8
1804af805 call     0x180308eb0 ; 
1804af80b call     0x180309d30 ; 

FUNCTION 9692 IotaInventory EnsureSorted 0x1804ae6e0 16
1804ae6e0 cmp      byte ptr [rcx + 0x28], 0
1804ae6e4 je       0x1804ae6ed
1804ae6e6 xor      edx, edx
1804ae6e8 jmp      0x1804af8d0 ; 9692:ifapp.Game.Data.IotaInventory.Sort
1804ae6ed ret      

FUNCTION 9692 IotaInventory Sort 0x1804af8d0 192
1804af8d0 push     rbx
1804af8d2 sub      rsp, 0x30
1804af8d6 cmp      byte ptr [rip + 0x2e8dd8e], 0
1804af8dd mov      rbx, rcx
1804af8e0 jne      0x1804af919
1804af8e2 lea      rcx, [rip + 0x2cf20af]
1804af8e9 call     0x180309af0 ; 
1804af8ee lea      rcx, [rip + 0x2d0e2eb]
1804af8f5 call     0x180309af0 ; 
1804af8fa lea      rcx, [rip + 0x2cc5a87]
1804af901 call     0x180309af0 ; 
1804af906 lea      rcx, [rip + 0x2cc5b13]
1804af90d call     0x180309af0 ; 
1804af912 mov      byte ptr [rip + 0x2e8dd52], 1
1804af919 mov      rax, qword ptr [rbx + 0x10]
1804af91d test     rax, rax
1804af920 je       0x1804af98a
1804af922 mov      r9, qword ptr [rip + 0x2d0e2b7]
1804af929 mov      qword ptr [rsp + 0x40], rsi
1804af92e mov      esi, dword ptr [rax + 0x18]
1804af931 mov      qword ptr [rsp + 0x48], rdi
1804af936 cmp      dword ptr [r9 + 0xe4], 0
1804af93e mov      rdi, qword ptr [rax + 0x10]
1804af942 jne      0x1804af953
1804af944 mov      rcx, r9
1804af947 call     0x180309de0 ; 
1804af94c mov      r9, qword ptr [rip + 0x2d0e28d]
1804af953 mov      r9, qword ptr [r9 + 0xb8]
1804af95a mov      r8d, esi
1804af95d mov      rax, qword ptr [rip + 0x2cf2034]
1804af964 xor      edx, edx
1804af966 mov      rcx, rdi
1804af969 mov      qword ptr [rsp + 0x20], rax
1804af96e mov      r9, qword ptr [r9]
1804af971 call     0x1807d2d20 ; 
1804af976 mov      rdi, qword ptr [rsp + 0x48]
1804af97b mov      rsi, qword ptr [rsp + 0x40]
1804af980 mov      byte ptr [rbx + 0x28], 0
1804af984 add      rsp, 0x30
1804af988 pop      rbx
1804af989 ret      
1804af98a call     0x180309d40 ; 

FUNCTION 9692 IotaInventory Initialize 0x1804aeb50 128
1804aeb50 push     rdi
1804aeb52 sub      rsp, 0x20
1804aeb56 cmp      byte ptr [rip + 0x2e8eb0f], 0
1804aeb5d mov      rdi, rcx
1804aeb60 jne      0x1804aeb81
1804aeb62 lea      rcx, [rip + 0x2cc63ff]
1804aeb69 call     0x180309af0 ; 
1804aeb6e lea      rcx, [rip + 0x2d192db]
1804aeb75 call     0x180309af0 ; 
1804aeb7a mov      byte ptr [rip + 0x2e8eaeb], 1
1804aeb81 cmp      qword ptr [rdi + 0x10], 0
1804aeb86 jne      0x1804aebb9
1804aeb88 mov      rcx, qword ptr [rip + 0x2d192c1]
1804aeb8f mov      qword ptr [rsp + 0x30], rbx
1804aeb94 call     0x180309ce0 ; 
1804aeb99 mov      r8, qword ptr [rip + 0x2cc63c8]
1804aeba0 mov      edx, 8
1804aeba5 mov      rcx, rax
1804aeba8 mov      rbx, rax
1804aebab call     0x180fa5800 ; 
1804aebb0 mov      qword ptr [rdi + 0x10], rbx
1804aebb4 mov      rbx, qword ptr [rsp + 0x30]
1804aebb9 xor      edx, edx
1804aebbb mov      byte ptr [rdi + 0x28], 1
1804aebbf mov      rcx, rdi
1804aebc2 add      rsp, 0x20
1804aebc6 pop      rdi
1804aebc7 jmp      0x1804af8d0 ; 9692:ifapp.Game.Data.IotaInventory.Sort

FUNCTION 9692 IotaInventory .cctor 0x1804af990 2848
1804af990 mov      qword ptr [rsp + 8], rbx
1804af995 push     rdi
1804af996 sub      rsp, 0x20
1804af99a cmp      byte ptr [rip + 0x2e8dccc], 0
1804af9a1 jne      0x1804afab6
1804af9a7 lea      rcx, [rip + 0x2cbcfea]
1804af9ae call     0x180309af0 ; 
1804af9b3 lea      rcx, [rip + 0x2cbd076]
1804af9ba call     0x180309af0 ; 
1804af9bf lea      rcx, [rip + 0x2cbd102]
1804af9c6 call     0x180309af0 ; 
1804af9cb lea      rcx, [rip + 0x2cbd18e]
1804af9d2 call     0x180309af0 ; 
1804af9d7 lea      rcx, [rip + 0x2cbd21a]
1804af9de call     0x180309af0 ; 
1804af9e3 lea      rcx, [rip + 0x2cbd2a6]
1804af9ea call     0x180309af0 ; 
1804af9ef lea      rcx, [rip + 0x2cbd332]
1804af9f6 call     0x180309af0 ; 
1804af9fb lea      rcx, [rip + 0x2cbd3be]
1804afa02 call     0x180309af0 ; 
1804afa07 lea      rcx, [rip + 0x2cbd44a]
1804afa0e call     0x180309af0 ; 
1804afa13 lea      rcx, [rip + 0x2cbd4d6]
1804afa1a call     0x180309af0 ; 
1804afa1f lea      rcx, [rip + 0x2d1e32a]
1804afa26 call     0x180309af0 ; 
1804afa2b lea      rcx, [rip + 0x2d1e44e]
1804afa32 call     0x180309af0 ; 
1804afa37 lea      rcx, [rip + 0x2d1e27a]
1804afa3e call     0x180309af0 ; 
1804afa43 lea      rcx, [rip + 0x2d0e196]
1804afa4a call     0x180309af0 ; 
1804afa4f lea      rcx, [rip + 0x2cbd5ca]
1804afa56 call     0x180309af0 ; 
1804afa5b lea      rcx, [rip + 0x2cbd656]
1804afa62 call     0x180309af0 ; 
1804afa67 lea      rcx, [rip + 0x2cbd6e2]
1804afa6e call     0x180309af0 ; 
1804afa73 lea      rcx, [rip + 0x2cbd76e]
1804afa7a call     0x180309af0 ; 
1804afa7f lea      rcx, [rip + 0x2cbd7fa]
1804afa86 call     0x180309af0 ; 
1804afa8b lea      rcx, [rip + 0x2cbd886]
1804afa92 call     0x180309af0 ; 
1804afa97 lea      rcx, [rip + 0x2cbd912]
1804afa9e call     0x180309af0 ; 
1804afaa3 lea      rcx, [rip + 0x2cbd99e]
1804afaaa call     0x180309af0 ; 
1804afaaf mov      byte ptr [rip + 0x2e8dbb7], 1
1804afab6 mov      rcx, qword ptr [rip + 0x2cbd00b]
1804afabd call     0x180309ce0 ; 
1804afac2 xor      edx, edx
1804afac4 mov      rcx, rax
1804afac7 mov      rbx, rax
1804afaca call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afacf mov      rcx, qword ptr [rip + 0x2d0e10a]
1804afad6 mov      rdx, qword ptr [rcx + 0xb8]
1804afadd mov      qword ptr [rdx], rbx
1804afae0 mov      edx, 0xa
1804afae5 mov      rcx, qword ptr [rip + 0x2d1e264]
1804afaec call     0x180308ef0 ; 
1804afaf1 mov      rcx, qword ptr [rip + 0x2cbd230]
1804afaf8 mov      rbx, rax
1804afafb call     0x180309ce0 ; 
1804afb00 xor      edx, edx
1804afb02 mov      rcx, rax
1804afb05 mov      rdi, rax
1804afb08 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afb0d test     rbx, rbx
1804afb10 je       0x1804b02de
1804afb16 test     rdi, rdi
1804afb19 je       0x1804afb33
1804afb1b mov      rdx, qword ptr [rbx]
1804afb1e mov      rcx, rdi
1804afb21 mov      rdx, qword ptr [rdx + 0x40]
1804afb25 call     0x180308e90 ; 
1804afb2a test     rax, rax
1804afb2d je       0x1804b02e4
1804afb33 cmp      dword ptr [rbx + 0x18], 0
1804afb37 jbe      0x1804b04a4
1804afb3d mov      qword ptr [rbx + 0x20], rdi
1804afb41 mov      rcx, qword ptr [rip + 0x2cbd278]
1804afb48 call     0x180309ce0 ; 
1804afb4d xor      edx, edx
1804afb4f mov      rcx, rax
1804afb52 mov      rdi, rax
1804afb55 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afb5a test     rdi, rdi
1804afb5d je       0x1804afb77
1804afb5f mov      rdx, qword ptr [rbx]
1804afb62 mov      rcx, rdi
1804afb65 mov      rdx, qword ptr [rdx + 0x40]
1804afb69 call     0x180308e90 ; 
1804afb6e test     rax, rax
1804afb71 je       0x1804b02f4
1804afb77 cmp      dword ptr [rbx + 0x18], 1
1804afb7b jbe      0x1804b04a4
1804afb81 mov      qword ptr [rbx + 0x28], rdi
1804afb85 mov      rcx, qword ptr [rip + 0x2cbce0c]
1804afb8c call     0x180309ce0 ; 
1804afb91 xor      edx, edx
1804afb93 mov      rcx, rax
1804afb96 mov      rdi, rax
1804afb99 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afb9e test     rdi, rdi
1804afba1 je       0x1804afbbb
1804afba3 mov      rdx, qword ptr [rbx]
1804afba6 mov      rcx, rdi
1804afba9 mov      rdx, qword ptr [rdx + 0x40]
1804afbad call     0x180308e90 ; 
1804afbb2 test     rax, rax
1804afbb5 je       0x1804b0304
1804afbbb cmp      dword ptr [rbx + 0x18], 2
1804afbbf jbe      0x1804b04a4
1804afbc5 mov      qword ptr [rbx + 0x30], rdi
1804afbc9 mov      rcx, qword ptr [rip + 0x2cbce60]
1804afbd0 call     0x180309ce0 ; 
1804afbd5 xor      edx, edx
1804afbd7 mov      rcx, rax
1804afbda mov      rdi, rax
1804afbdd call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afbe2 test     rdi, rdi
1804afbe5 je       0x1804afbff
1804afbe7 mov      rdx, qword ptr [rbx]
1804afbea mov      rcx, rdi
1804afbed mov      rdx, qword ptr [rdx + 0x40]
1804afbf1 call     0x180308e90 ; 
1804afbf6 test     rax, rax
1804afbf9 je       0x1804b0314
1804afbff cmp      dword ptr [rbx + 0x18], 3
1804afc03 jbe      0x1804b04a4
1804afc09 mov      qword ptr [rbx + 0x38], rdi
1804afc0d mov      rcx, qword ptr [rip + 0x2cbcfe4]
1804afc14 call     0x180309ce0 ; 
1804afc19 xor      edx, edx
1804afc1b mov      rcx, rax
1804afc1e mov      rdi, rax
1804afc21 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afc26 test     rdi, rdi
1804afc29 je       0x1804afc43
1804afc2b mov      rdx, qword ptr [rbx]
1804afc2e mov      rcx, rdi
1804afc31 mov      rdx, qword ptr [rdx + 0x40]
1804afc35 call     0x180308e90 ; 
1804afc3a test     rax, rax
1804afc3d je       0x1804b0324
1804afc43 cmp      dword ptr [rbx + 0x18], 4
1804afc47 jbe      0x1804b04a4
1804afc4d mov      qword ptr [rbx + 0x40], rdi
1804afc51 mov      rcx, qword ptr [rip + 0x2cbd038]
1804afc58 call     0x180309ce0 ; 
1804afc5d xor      edx, edx
1804afc5f mov      rcx, rax
1804afc62 mov      rdi, rax
1804afc65 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afc6a test     rdi, rdi
1804afc6d je       0x1804afc87
1804afc6f mov      rdx, qword ptr [rbx]
1804afc72 mov      rcx, rdi
1804afc75 mov      rdx, qword ptr [rdx + 0x40]
1804afc79 call     0x180308e90 ; 
1804afc7e test     rax, rax
1804afc81 je       0x1804b0334
1804afc87 cmp      dword ptr [rbx + 0x18], 5
1804afc8b jbe      0x1804b04a4
1804afc91 mov      qword ptr [rbx + 0x48], rdi
1804afc95 mov      rcx, qword ptr [rip + 0x2cbd384]
1804afc9c call     0x180309ce0 ; 
1804afca1 xor      edx, edx
1804afca3 mov      rcx, rax
1804afca6 mov      rdi, rax
1804afca9 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afcae test     rdi, rdi
1804afcb1 je       0x1804afccb
1804afcb3 mov      rdx, qword ptr [rbx]
1804afcb6 mov      rcx, rdi
1804afcb9 mov      rdx, qword ptr [rdx + 0x40]
1804afcbd call     0x180308e90 ; 
1804afcc2 test     rax, rax
1804afcc5 je       0x1804b0344
1804afccb cmp      dword ptr [rbx + 0x18], 6
1804afccf jbe      0x1804b04a4
1804afcd5 mov      qword ptr [rbx + 0x50], rdi
1804afcd9 mov      rcx, qword ptr [rip + 0x2cbd470]
1804afce0 call     0x180309ce0 ; 
1804afce5 xor      edx, edx
1804afce7 mov      rcx, rax
1804afcea mov      rdi, rax
1804afced call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afcf2 test     rdi, rdi
1804afcf5 je       0x1804afd0f
1804afcf7 mov      rdx, qword ptr [rbx]
1804afcfa mov      rcx, rdi
1804afcfd mov      rdx, qword ptr [rdx + 0x40]
1804afd01 call     0x180308e90 ; 
1804afd06 test     rax, rax
1804afd09 je       0x1804b0354
1804afd0f cmp      dword ptr [rbx + 0x18], 7
1804afd13 jbe      0x1804b04a4
1804afd19 mov      qword ptr [rbx + 0x58], rdi
1804afd1d mov      rax, qword ptr [rip + 0x2d0debc]
1804afd24 mov      rcx, qword ptr [rax + 0xb8]
1804afd2b mov      rdi, qword ptr [rcx]
1804afd2e test     rdi, rdi
1804afd31 je       0x1804afd4b
1804afd33 mov      rdx, qword ptr [rbx]
1804afd36 mov      rcx, rdi
1804afd39 mov      rdx, qword ptr [rdx + 0x40]
1804afd3d call     0x180308e90 ; 
1804afd42 test     rax, rax
1804afd45 je       0x1804b0364
1804afd4b cmp      dword ptr [rbx + 0x18], 8
1804afd4f jbe      0x1804b04a4
1804afd55 mov      qword ptr [rbx + 0x60], rdi
1804afd59 mov      rcx, qword ptr [rip + 0x2cbce00]
1804afd60 call     0x180309ce0 ; 
1804afd65 xor      edx, edx
1804afd67 mov      rcx, rax
1804afd6a mov      rdi, rax
1804afd6d call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afd72 test     rdi, rdi
1804afd75 je       0x1804afd8f
1804afd77 mov      rdx, qword ptr [rbx]
1804afd7a mov      rcx, rdi
1804afd7d mov      rdx, qword ptr [rdx + 0x40]
1804afd81 call     0x180308e90 ; 
1804afd86 test     rax, rax
1804afd89 je       0x1804b0374
1804afd8f cmp      dword ptr [rbx + 0x18], 9
1804afd93 jbe      0x1804b04a4
1804afd99 mov      qword ptr [rbx + 0x68], rdi
1804afd9d mov      edx, 0xa
1804afda2 mov      rax, qword ptr [rip + 0x2d0de37]
1804afda9 mov      rcx, qword ptr [rax + 0xb8]
1804afdb0 mov      qword ptr [rcx + 8], rbx
1804afdb4 mov      rcx, qword ptr [rip + 0x2d1e0c5]
1804afdbb call     0x180308ef0 ; 
1804afdc0 mov      rcx, qword ptr [rip + 0x2cbcf61]
1804afdc7 mov      rbx, rax
1804afdca call     0x180309ce0 ; 
1804afdcf xor      edx, edx
1804afdd1 mov      rcx, rax
1804afdd4 mov      rdi, rax
1804afdd7 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afddc test     rbx, rbx
1804afddf je       0x1804b02de
1804afde5 test     rdi, rdi
1804afde8 je       0x1804afe02
1804afdea mov      rdx, qword ptr [rbx]
1804afded mov      rcx, rdi
1804afdf0 mov      rdx, qword ptr [rdx + 0x40]
1804afdf4 call     0x180308e90 ; 
1804afdf9 test     rax, rax
1804afdfc je       0x1804b0384
1804afe02 cmp      dword ptr [rbx + 0x18], 0
1804afe06 jbe      0x1804b04a4
1804afe0c mov      qword ptr [rbx + 0x20], rdi
1804afe10 mov      rcx, qword ptr [rip + 0x2cbcfa9]
1804afe17 call     0x180309ce0 ; 
1804afe1c xor      edx, edx
1804afe1e mov      rcx, rax
1804afe21 mov      rdi, rax
1804afe24 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afe29 test     rdi, rdi
1804afe2c je       0x1804afe46
1804afe2e mov      rdx, qword ptr [rbx]
1804afe31 mov      rcx, rdi
1804afe34 mov      rdx, qword ptr [rdx + 0x40]
1804afe38 call     0x180308e90 ; 
1804afe3d test     rax, rax
1804afe40 je       0x1804b0394
1804afe46 cmp      dword ptr [rbx + 0x18], 1
1804afe4a jbe      0x1804b04a4
1804afe50 mov      qword ptr [rbx + 0x28], rdi
1804afe54 mov      rcx, qword ptr [rip + 0x2cbcb3d]
1804afe5b call     0x180309ce0 ; 
1804afe60 xor      edx, edx
1804afe62 mov      rcx, rax
1804afe65 mov      rdi, rax
1804afe68 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afe6d test     rdi, rdi
1804afe70 je       0x1804afe8a
1804afe72 mov      rdx, qword ptr [rbx]
1804afe75 mov      rcx, rdi
1804afe78 mov      rdx, qword ptr [rdx + 0x40]
1804afe7c call     0x180308e90 ; 
1804afe81 test     rax, rax
1804afe84 je       0x1804b03a4
1804afe8a cmp      dword ptr [rbx + 0x18], 2
1804afe8e jbe      0x1804b04a4
1804afe94 mov      qword ptr [rbx + 0x30], rdi
1804afe98 mov      rcx, qword ptr [rip + 0x2cbcb91]
1804afe9f call     0x180309ce0 ; 
1804afea4 xor      edx, edx
1804afea6 mov      rcx, rax
1804afea9 mov      rdi, rax
1804afeac call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afeb1 test     rdi, rdi
1804afeb4 je       0x1804afece
1804afeb6 mov      rdx, qword ptr [rbx]
1804afeb9 mov      rcx, rdi
1804afebc mov      rdx, qword ptr [rdx + 0x40]
1804afec0 call     0x180308e90 ; 
1804afec5 test     rax, rax
1804afec8 je       0x1804b03b4
1804afece cmp      dword ptr [rbx + 0x18], 3
1804afed2 jbe      0x1804b04a4
1804afed8 mov      qword ptr [rbx + 0x38], rdi
1804afedc mov      rcx, qword ptr [rip + 0x2cbcd15]
1804afee3 call     0x180309ce0 ; 
1804afee8 xor      edx, edx
1804afeea mov      rcx, rax
1804afeed mov      rdi, rax
1804afef0 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804afef5 test     rdi, rdi
1804afef8 je       0x1804aff12
1804afefa mov      rdx, qword ptr [rbx]
1804afefd mov      rcx, rdi
1804aff00 mov      rdx, qword ptr [rdx + 0x40]
1804aff04 call     0x180308e90 ; 
1804aff09 test     rax, rax
1804aff0c je       0x1804b03c4
1804aff12 cmp      dword ptr [rbx + 0x18], 4
1804aff16 jbe      0x1804b04a4
1804aff1c mov      qword ptr [rbx + 0x40], rdi
1804aff20 mov      rcx, qword ptr [rip + 0x2cbcd69]
1804aff27 call     0x180309ce0 ; 
1804aff2c xor      edx, edx
1804aff2e mov      rcx, rax
1804aff31 mov      rdi, rax
1804aff34 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804aff39 test     rdi, rdi
1804aff3c je       0x1804aff56
1804aff3e mov      rdx, qword ptr [rbx]
1804aff41 mov      rcx, rdi
1804aff44 mov      rdx, qword ptr [rdx + 0x40]
1804aff48 call     0x180308e90 ; 
1804aff4d test     rax, rax
1804aff50 je       0x1804b03d4
1804aff56 cmp      dword ptr [rbx + 0x18], 5
1804aff5a jbe      0x1804b04a4
1804aff60 mov      qword ptr [rbx + 0x48], rdi
1804aff64 mov      rcx, qword ptr [rip + 0x2cbd0b5]
1804aff6b call     0x180309ce0 ; 
1804aff70 xor      edx, edx
1804aff72 mov      rcx, rax
1804aff75 mov      rdi, rax
1804aff78 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804aff7d test     rdi, rdi
1804aff80 je       0x1804aff9a
1804aff82 mov      rdx, qword ptr [rbx]
1804aff85 mov      rcx, rdi
1804aff88 mov      rdx, qword ptr [rdx + 0x40]
1804aff8c call     0x180308e90 ; 
1804aff91 test     rax, rax
1804aff94 je       0x1804b03e4
1804aff9a cmp      dword ptr [rbx + 0x18], 6
1804aff9e jbe      0x1804b04a4
1804affa4 mov      qword ptr [rbx + 0x50], rdi
1804affa8 mov      rcx, qword ptr [rip + 0x2cbd1a1]
1804affaf call     0x180309ce0 ; 
1804affb4 xor      edx, edx
1804affb6 mov      rcx, rax
1804affb9 mov      rdi, rax
1804affbc call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804affc1 test     rdi, rdi
1804affc4 je       0x1804affde
1804affc6 mov      rdx, qword ptr [rbx]
1804affc9 mov      rcx, rdi
1804affcc mov      rdx, qword ptr [rdx + 0x40]
1804affd0 call     0x180308e90 ; 
1804affd5 test     rax, rax
1804affd8 je       0x1804b03f4
1804affde cmp      dword ptr [rbx + 0x18], 7
1804affe2 jbe      0x1804b04a4
1804affe8 mov      qword ptr [rbx + 0x58], rdi
1804affec mov      rax, qword ptr [rip + 0x2d0dbed]
1804afff3 mov      rcx, qword ptr [rax + 0xb8]
1804afffa mov      rdi, qword ptr [rcx]
1804afffd test     rdi, rdi
1804b0000 je       0x1804b001a
1804b0002 mov      rdx, qword ptr [rbx]
1804b0005 mov      rcx, rdi
1804b0008 mov      rdx, qword ptr [rdx + 0x40]
1804b000c call     0x180308e90 ; 
1804b0011 test     rax, rax
1804b0014 je       0x1804b0404
1804b001a cmp      dword ptr [rbx + 0x18], 8
1804b001e jbe      0x1804b04a4
1804b0024 mov      qword ptr [rbx + 0x60], rdi
1804b0028 mov      rcx, qword ptr [rip + 0x2cbcb31]
1804b002f call     0x180309ce0 ; 
1804b0034 xor      edx, edx
1804b0036 mov      rcx, rax
1804b0039 mov      rdi, rax
1804b003c call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b0041 test     rdi, rdi
1804b0044 je       0x1804b005e
1804b0046 mov      rdx, qword ptr [rbx]
1804b0049 mov      rcx, rdi
1804b004c mov      rdx, qword ptr [rdx + 0x40]
1804b0050 call     0x180308e90 ; 
1804b0055 test     rax, rax
1804b0058 je       0x1804b0414
1804b005e cmp      dword ptr [rbx + 0x18], 9
1804b0062 jbe      0x1804b04a4
1804b0068 mov      qword ptr [rbx + 0x68], rdi
1804b006c mov      edx, 8
1804b0071 mov      rax, qword ptr [rip + 0x2d0db68]
1804b0078 mov      rcx, qword ptr [rax + 0xb8]
1804b007f mov      qword ptr [rcx + 0x10], rbx
1804b0083 mov      rcx, qword ptr [rip + 0x2d1dc2e]
1804b008a call     0x180308ef0 ; 
1804b008f mov      rcx, qword ptr [rip + 0x2cbd31a]
1804b0096 mov      rbx, rax
1804b0099 call     0x180309ce0 ; 
1804b009e xor      edx, edx
1804b00a0 mov      rcx, rax
1804b00a3 mov      rdi, rax
1804b00a6 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b00ab test     rbx, rbx
1804b00ae je       0x1804b02de
1804b00b4 test     rdi, rdi
1804b00b7 je       0x1804b00d1
1804b00b9 mov      rdx, qword ptr [rbx]
1804b00bc mov      rcx, rdi
1804b00bf mov      rdx, qword ptr [rdx + 0x40]
1804b00c3 call     0x180308e90 ; 
1804b00c8 test     rax, rax
1804b00cb je       0x1804b0424
1804b00d1 cmp      dword ptr [rbx + 0x18], 0
1804b00d5 jbe      0x1804b04a4
1804b00db mov      qword ptr [rbx + 0x20], rdi
1804b00df mov      rcx, qword ptr [rip + 0x2cbd362]
1804b00e6 call     0x180309ce0 ; 
1804b00eb xor      edx, edx
1804b00ed mov      rcx, rax
1804b00f0 mov      rdi, rax
1804b00f3 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b00f8 test     rdi, rdi
1804b00fb je       0x1804b0115
1804b00fd mov      rdx, qword ptr [rbx]
1804b0100 mov      rcx, rdi
1804b0103 mov      rdx, qword ptr [rdx + 0x40]
1804b0107 call     0x180308e90 ; 
1804b010c test     rax, rax
1804b010f je       0x1804b0434
1804b0115 cmp      dword ptr [rbx + 0x18], 1
1804b0119 jbe      0x1804b04a4
1804b011f mov      qword ptr [rbx + 0x28], rdi
1804b0123 mov      rcx, qword ptr [rip + 0x2cbcd2e]
1804b012a call     0x180309ce0 ; 
1804b012f xor      edx, edx
1804b0131 mov      rcx, rax
1804b0134 mov      rdi, rax
1804b0137 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b013c test     rdi, rdi
1804b013f je       0x1804b0159
1804b0141 mov      rdx, qword ptr [rbx]
1804b0144 mov      rcx, rdi
1804b0147 mov      rdx, qword ptr [rdx + 0x40]
1804b014b call     0x180308e90 ; 
1804b0150 test     rax, rax
1804b0153 je       0x1804b0444
1804b0159 cmp      dword ptr [rbx + 0x18], 2
1804b015d jbe      0x1804b04a4
1804b0163 mov      qword ptr [rbx + 0x30], rdi
1804b0167 mov      rcx, qword ptr [rip + 0x2cbcd82]
1804b016e call     0x180309ce0 ; 
1804b0173 xor      edx, edx
1804b0175 mov      rcx, rax
1804b0178 mov      rdi, rax
1804b017b call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b0180 test     rdi, rdi
1804b0183 je       0x1804b019d
1804b0185 mov      rdx, qword ptr [rbx]
1804b0188 mov      rcx, rdi
1804b018b mov      rdx, qword ptr [rdx + 0x40]
1804b018f call     0x180308e90 ; 
1804b0194 test     rax, rax
1804b0197 je       0x1804b0454
1804b019d cmp      dword ptr [rbx + 0x18], 3
1804b01a1 jbe      0x1804b04a4
1804b01a7 mov      qword ptr [rbx + 0x38], rdi
1804b01ab mov      rcx, qword ptr [rip + 0x2cbd0ce]
1804b01b2 call     0x180309ce0 ; 
1804b01b7 xor      edx, edx
1804b01b9 mov      rcx, rax
1804b01bc mov      rdi, rax
1804b01bf call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b01c4 test     rdi, rdi
1804b01c7 je       0x1804b01e1
1804b01c9 mov      rdx, qword ptr [rbx]
1804b01cc mov      rcx, rdi
1804b01cf mov      rdx, qword ptr [rdx + 0x40]
1804b01d3 call     0x180308e90 ; 
1804b01d8 test     rax, rax
1804b01db je       0x1804b0464
1804b01e1 cmp      dword ptr [rbx + 0x18], 4
1804b01e5 jbe      0x1804b04a4
1804b01eb mov      qword ptr [rbx + 0x40], rdi
1804b01ef mov      rcx, qword ptr [rip + 0x2cbd122]
1804b01f6 call     0x180309ce0 ; 
1804b01fb xor      edx, edx
1804b01fd mov      rcx, rax
1804b0200 mov      rdi, rax
1804b0203 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b0208 test     rdi, rdi
1804b020b je       0x1804b0225
1804b020d mov      rdx, qword ptr [rbx]
1804b0210 mov      rcx, rdi
1804b0213 mov      rdx, qword ptr [rdx + 0x40]
1804b0217 call     0x180308e90 ; 
1804b021c test     rax, rax
1804b021f je       0x1804b0474
1804b0225 cmp      dword ptr [rbx + 0x18], 5
1804b0229 jbe      0x1804b04a4
1804b022f mov      qword ptr [rbx + 0x48], rdi
1804b0233 mov      rcx, qword ptr [rip + 0x2cbce7e]
1804b023a call     0x180309ce0 ; 
1804b023f xor      edx, edx
1804b0241 mov      rcx, rax
1804b0244 mov      rdi, rax
1804b0247 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b024c test     rdi, rdi
1804b024f je       0x1804b0269
1804b0251 mov      rdx, qword ptr [rbx]
1804b0254 mov      rcx, rdi
1804b0257 mov      rdx, qword ptr [rdx + 0x40]
1804b025b call     0x180308e90 ; 
1804b0260 test     rax, rax
1804b0263 je       0x1804b0484
1804b0269 cmp      dword ptr [rbx + 0x18], 6
1804b026d jbe      0x1804b04a4
1804b0273 mov      qword ptr [rbx + 0x50], rdi
1804b0277 mov      rcx, qword ptr [rip + 0x2cbcf6a]
1804b027e call     0x180309ce0 ; 
1804b0283 xor      edx, edx
1804b0285 mov      rcx, rax
1804b0288 mov      rdi, rax
1804b028b call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804b0290 test     rdi, rdi
1804b0293 je       0x1804b02ad
1804b0295 mov      rdx, qword ptr [rbx]
1804b0298 mov      rcx, rdi
1804b029b mov      rdx, qword ptr [rdx + 0x40]
1804b029f call     0x180308e90 ; 
1804b02a4 test     rax, rax
1804b02a7 je       0x1804b0494
1804b02ad cmp      dword ptr [rbx + 0x18], 7
1804b02b1 jbe      0x1804b04a4
1804b02b7 mov      qword ptr [rbx + 0x58], rdi
1804b02bb mov      rax, qword ptr [rip + 0x2d0d91e]
1804b02c2 mov      rcx, qword ptr [rax + 0xb8]
1804b02c9 mov      qword ptr [rcx + 0x18], rbx
1804b02cd xor      ecx, ecx
1804b02cf mov      rbx, qword ptr [rsp + 0x30]
1804b02d4 add      rsp, 0x20
1804b02d8 pop      rdi
1804b02d9 jmp      0x1804aecb0 ; 9692:ifapp.Game.Data.IotaInventory.RegisterFormatter
1804b02de call     0x180309d40 ; 
1804b02e4 call     0x180309730 ; 
1804b02e9 mov      rcx, rax
1804b02ec xor      edx, edx
1804b02ee call     0x180309d00 ; 
1804b02f4 call     0x180309730 ; 
1804b02f9 mov      rcx, rax
1804b02fc xor      edx, edx
1804b02fe call     0x180309d00 ; 
1804b0304 call     0x180309730 ; 
1804b0309 mov      rcx, rax
1804b030c xor      edx, edx
1804b030e call     0x180309d00 ; 
1804b0314 call     0x180309730 ; 
1804b0319 mov      rcx, rax
1804b031c xor      edx, edx
1804b031e call     0x180309d00 ; 
1804b0324 call     0x180309730 ; 
1804b0329 mov      rcx, rax
1804b032c xor      edx, edx
1804b032e call     0x180309d00 ; 
1804b0334 call     0x180309730 ; 
1804b0339 mov      rcx, rax
1804b033c xor      edx, edx
1804b033e call     0x180309d00 ; 
1804b0344 call     0x180309730 ; 
1804b0349 mov      rcx, rax
1804b034c xor      edx, edx
1804b034e call     0x180309d00 ; 
1804b0354 call     0x180309730 ; 
1804b0359 mov      rcx, rax
1804b035c xor      edx, edx
1804b035e call     0x180309d00 ; 
1804b0364 call     0x180309730 ; 
1804b0369 mov      rcx, rax
1804b036c xor      edx, edx
1804b036e call     0x180309d00 ; 
1804b0374 call     0x180309730 ; 
1804b0379 mov      rcx, rax
1804b037c xor      edx, edx
1804b037e call     0x180309d00 ; 
1804b0384 call     0x180309730 ; 
1804b0389 mov      rcx, rax
1804b038c xor      edx, edx
1804b038e call     0x180309d00 ; 
1804b0394 call     0x180309730 ; 
1804b0399 mov      rcx, rax
1804b039c xor      edx, edx
1804b039e call     0x180309d00 ; 
1804b03a4 call     0x180309730 ; 
1804b03a9 mov      rcx, rax
1804b03ac xor      edx, edx
1804b03ae call     0x180309d00 ; 
1804b03b4 call     0x180309730 ; 
1804b03b9 mov      rcx, rax
1804b03bc xor      edx, edx
1804b03be call     0x180309d00 ; 
1804b03c4 call     0x180309730 ; 
1804b03c9 mov      rcx, rax
1804b03cc xor      edx, edx
1804b03ce call     0x180309d00 ; 
1804b03d4 call     0x180309730 ; 
1804b03d9 mov      rcx, rax
1804b03dc xor      edx, edx
1804b03de call     0x180309d00 ; 
1804b03e4 call     0x180309730 ; 
1804b03e9 mov      rcx, rax
1804b03ec xor      edx, edx
1804b03ee call     0x180309d00 ; 
1804b03f4 call     0x180309730 ; 
1804b03f9 mov      rcx, rax
1804b03fc xor      edx, edx
1804b03fe call     0x180309d00 ; 
1804b0404 call     0x180309730 ; 
1804b0409 mov      rcx, rax
1804b040c xor      edx, edx
1804b040e call     0x180309d00 ; 
1804b0414 call     0x180309730 ; 
1804b0419 mov      rcx, rax
1804b041c xor      edx, edx
1804b041e call     0x180309d00 ; 
1804b0424 call     0x180309730 ; 
1804b0429 mov      rcx, rax
1804b042c xor      edx, edx
1804b042e call     0x180309d00 ; 
1804b0434 call     0x180309730 ; 
1804b0439 mov      rcx, rax
1804b043c xor      edx, edx
1804b043e call     0x180309d00 ; 
1804b0444 call     0x180309730 ; 
1804b0449 mov      rcx, rax
1804b044c xor      edx, edx
1804b044e call     0x180309d00 ; 
1804b0454 call     0x180309730 ; 
1804b0459 mov      rcx, rax
1804b045c xor      edx, edx
1804b045e call     0x180309d00 ; 
1804b0464 call     0x180309730 ; 
1804b0469 mov      rcx, rax
1804b046c xor      edx, edx
1804b046e call     0x180309d00 ; 
1804b0474 call     0x180309730 ; 
1804b0479 mov      rcx, rax
1804b047c xor      edx, edx
1804b047e call     0x180309d00 ; 
1804b0484 call     0x180309730 ; 
1804b0489 mov      rcx, rax
1804b048c xor      edx, edx
1804b048e call     0x180309d00 ; 
1804b0494 call     0x180309730 ; 
1804b0499 mov      rcx, rax
1804b049c xor      edx, edx
1804b049e call     0x180309d00 ; 
1804b04a4 call     0x180309d30 ; 

FUNCTION 9692 IotaInventory RegisterFormatter 0x1804aecb0 656
1804aecb0 push     rbx
1804aecb2 sub      rsp, 0x20
1804aecb6 cmp      byte ptr [rip + 0x2e8e9b1], 0
1804aecbd jne      0x1804aed5a
1804aecc3 lea      rcx, [rip + 0x2cf5526]
1804aecca call     0x180309af0 ; 
1804aeccf lea      rcx, [rip + 0x2d1b612]
1804aecd6 call     0x180309af0 ; 
1804aecdb lea      rcx, [rip + 0x2d149ce]
1804aece2 call     0x180309af0 ; 
1804aece7 lea      rcx, [rip + 0x2cdfa3a]
1804aecee call     0x180309af0 ; 
1804aecf3 lea      rcx, [rip + 0x2cbe28e]
1804aecfa call     0x180309af0 ; 
1804aecff lea      rcx, [rip + 0x2cceab2]
1804aed06 call     0x180309af0 ; 
1804aed0b lea      rcx, [rip + 0x2ccf7b6]
1804aed12 call     0x180309af0 ; 
1804aed17 lea      rcx, [rip + 0x2cd14b2]
1804aed1e call     0x180309af0 ; 
1804aed23 lea      rcx, [rip + 0x2cd29f6]
1804aed2a call     0x180309af0 ; 
1804aed2f lea      rcx, [rip + 0x2cd5262]
1804aed36 call     0x180309af0 ; 
1804aed3b lea      rcx, [rip + 0x2cd840e]
1804aed42 call     0x180309af0 ; 
1804aed47 lea      rcx, [rip + 0x2d18682]
1804aed4e call     0x180309af0 ; 
1804aed53 mov      byte ptr [rip + 0x2e8e914], 1
1804aed5a mov      rcx, qword ptr [rip + 0x2d1866f]
1804aed61 cmp      dword ptr [rcx + 0xe4], 0
1804aed68 jne      0x1804aed6f
1804aed6a call     0x180309de0 ; 
1804aed6f mov      rbx, qword ptr [rip + 0x2cd145a]
1804aed76 cmp      qword ptr [rbx + 0x38], 0
1804aed7b jne      0x1804aed85
1804aed7d mov      rcx, rbx
1804aed80 call     0x18030dbb0 ; 
1804aed85 mov      rax, qword ptr [rbx + 0x38]
1804aed89 mov      rax, qword ptr [rax + 8]
1804aed8d test     byte ptr [rax + 0x135], 1
1804aed94 jne      0x1804aed9e
1804aed96 mov      rcx, rax
1804aed99 call     0x18030db30 ; 
1804aed9e mov      rax, qword ptr [rax + 0xb8]
1804aeda5 cmp      byte ptr [rax], 0
1804aeda8 jne      0x1804aee08
1804aedaa mov      rcx, qword ptr [rip + 0x2cbe1d7]
1804aedb1 call     0x180309ce0 ; 
1804aedb6 cmp      byte ptr [rip + 0x2e8e8c2], 0
1804aedbd mov      rbx, rax
1804aedc0 jne      0x1804aedd5
1804aedc2 lea      rcx, [rip + 0x2cbd977]
1804aedc9 call     0x180309af0 ; 
1804aedce mov      byte ptr [rip + 0x2e8e8aa], 1
1804aedd5 mov      rdx, qword ptr [rip + 0x2cbd964]
1804aeddc mov      rcx, rbx
1804aeddf call     0x180431940 ; 78:Mono.Globalization.Unicode.ContractionComparer..ctor | 80:.<>c..ctor | 89:Mono.Globalization.Unicode.SortKeyBuffer..ctor | 115:Mono.Math.Prime.Generator.PrimeGeneratorBase..ctor
1804aede4 mov      rcx, qword ptr [rip + 0x2d185e5]
1804aedeb cmp      dword ptr [rcx + 0xe4], 0
1804aedf2 jne      0x1804aedf9
1804aedf4 call     0x180309de0 ; 
1804aedf9 mov      rdx, qword ptr [rip + 0x2cd8350]
1804aee00 mov      rcx, rbx
1804aee03 call     0x1808b6330 ; 
1804aee08 mov      rcx, qword ptr [rip + 0x2d185c1]
1804aee0f cmp      dword ptr [rcx + 0xe4], 0
1804aee16 jne      0x1804aee1d
1804aee18 call     0x180309de0 ; 
1804aee1d mov      rbx, qword ptr [rip + 0x2ccf6a4]
1804aee24 cmp      qword ptr [rbx + 0x38], 0
1804aee29 jne      0x1804aee33
1804aee2b mov      rcx, rbx
1804aee2e call     0x18030dbb0 ; 
1804aee33 mov      rax, qword ptr [rbx + 0x38]
1804aee37 mov      rax, qword ptr [rax + 8]
1804aee3b test     byte ptr [rax + 0x135], 1
1804aee42 jne      0x1804aee4c
1804aee44 mov      rcx, rax
1804aee47 call     0x18030db30 ; 
1804aee4c mov      rax, qword ptr [rax + 0xb8]
1804aee53 cmp      byte ptr [rax], 0
1804aee56 jne      0x1804aee9a
1804aee58 mov      rcx, qword ptr [rip + 0x2d1b489]
1804aee5f call     0x180309ce0 ; 
1804aee64 mov      rdx, qword ptr [rip + 0x2cf5385]
1804aee6b mov      rcx, rax
1804aee6e mov      rbx, rax
1804aee71 call     0x180f9e230 ; 
1804aee76 mov      rcx, qword ptr [rip + 0x2d18553]
1804aee7d cmp      dword ptr [rcx + 0xe4], 0
1804aee84 jne      0x1804aee8b
1804aee86 call     0x180309de0 ; 
1804aee8b mov      rdx, qword ptr [rip + 0x2cd5106]
1804aee92 mov      rcx, rbx
1804aee95 call     0x1808b6330 ; 
1804aee9a mov      rcx, qword ptr [rip + 0x2d1852f]
1804aeea1 cmp      dword ptr [rcx + 0xe4], 0
1804aeea8 jne      0x1804aeeaf
1804aeeaa call     0x180309de0 ; 
1804aeeaf mov      rbx, qword ptr [rip + 0x2cce902]
1804aeeb6 cmp      qword ptr [rbx + 0x38], 0
1804aeebb jne      0x1804aeec5
1804aeebd mov      rcx, rbx
1804aeec0 call     0x18030dbb0 ; 
1804aeec5 mov      rax, qword ptr [rbx + 0x38]
1804aeec9 mov      rax, qword ptr [rax + 8]
1804aeecd test     byte ptr [rax + 0x135], 1
1804aeed4 jne      0x1804aeede
1804aeed6 mov      rcx, rax
1804aeed9 call     0x18030db30 ; 
1804aeede mov      rax, qword ptr [rax + 0xb8]
1804aeee5 cmp      byte ptr [rax], 0
1804aeee8 jne      0x1804aef31
1804aeeea mov      rcx, qword ptr [rip + 0x2cdf837]
1804aeef1 call     0x180309ce0 ; 
1804aeef6 mov      rdx, qword ptr [rip + 0x2d147b3]
1804aeefd mov      rcx, rax
1804aef00 mov      rbx, rax
1804aef03 call     0x1811dba80 ; 
1804aef08 mov      rcx, qword ptr [rip + 0x2d184c1]
1804aef0f cmp      dword ptr [rcx + 0xe4], 0
1804aef16 jne      0x1804aef1d
1804aef18 call     0x180309de0 ; 
1804aef1d mov      rdx, qword ptr [rip + 0x2cd27fc]
1804aef24 mov      rcx, rbx
1804aef27 add      rsp, 0x20
1804aef2b pop      rbx
1804aef2c jmp      0x1808b6330 ; 
1804aef31 add      rsp, 0x20
1804aef35 pop      rbx
1804aef36 ret      

FUNCTION 9692 IotaInventory Serialize 0x1804af3e0 304
1804af3e0 mov      qword ptr [rsp + 0x18], rbx
1804af3e5 push     rsi
1804af3e6 sub      rsp, 0x30
1804af3ea cmp      byte ptr [rip + 0x2e8e27e], 0
1804af3f1 mov      rsi, rdx
1804af3f4 mov      rbx, rcx
1804af3f7 jne      0x1804af418
1804af3f9 lea      rcx, [rip + 0x2cdf878]
1804af400 call     0x180309af0 ; 
1804af405 lea      rcx, [rip + 0x2ce1e94]
1804af40c call     0x180309af0 ; 
1804af411 mov      byte ptr [rip + 0x2e8e257], 1
1804af418 cmp      qword ptr [rsi], 0
1804af41c je       0x1804af4ec
1804af422 mov      qword ptr [rsp + 0x40], rbp
1804af427 cmp      byte ptr [rip + 0x2e8e1c5], 0
1804af42e jne      0x1804af443
1804af430 lea      rcx, [rip + 0x2ccd671]
1804af437 call     0x180309af0 ; 
1804af43c mov      byte ptr [rip + 0x2e8e1b0], 1
1804af443 cmp      dword ptr [rbx + 0x18], 1
1804af447 jge      0x1804af459
1804af449 xor      r8d, r8d
1804af44c mov      edx, 1
1804af451 mov      rcx, rbx
1804af454 call     0x1817077b0 ; 11113:MemoryPack.MemoryPackWriter.RequestNewBuffer
1804af459 movups   xmm0, xmmword ptr [rbx + 8]
1804af45d mov      rdx, qword ptr [rip + 0x2ccd644]
1804af464 lea      rcx, [rsp + 0x20]
1804af469 movaps   xmmword ptr [rsp + 0x20], xmm0
1804af46e call     0x1804192f0 ; 201:System.DateTime.System.IConvertible.ToDateTime | 257:System.Int64.System.IConvertible.ToInt64 | 315:System.TimeSpan.get_Ticks | 334:System.UInt64.System.IConvertible.ToUInt64
1804af473 xor      r8d, r8d
1804af476 mov      edx, 1
1804af47b mov      rcx, rbx
1804af47e mov      byte ptr [rax], 2
1804af481 call     0x180487e40 ; 
1804af486 mov      rbp, qword ptr [rsi]
1804af489 test     rbp, rbp
1804af48c je       0x1804af500
1804af48e mov      qword ptr [rsp + 0x48], rdi
1804af493 mov      rdi, qword ptr [rip + 0x2cdf7de]
1804af49a cmp      qword ptr [rdi + 0x38], 0
1804af49f jne      0x1804af4a9
1804af4a1 mov      rcx, rdi
1804af4a4 call     0x18030dbb0 ; 
1804af4a9 mov      r8, qword ptr [rdi + 0x38]
1804af4ad lea      rdx, [rbp + 0x10]
1804af4b1 mov      rcx, rbx
1804af4b4 mov      r8, qword ptr [r8 + 8]
1804af4b8 call     0x1804887f0 ; 
1804af4bd mov      rdx, qword ptr [rsi]
1804af4c0 mov      rdi, qword ptr [rsp + 0x48]
1804af4c5 test     rdx, rdx
1804af4c8 je       0x1804af500
1804af4ca mov      r8, qword ptr [rip + 0x2ce1dcf]
1804af4d1 add      rdx, 0x18
1804af4d5 mov      rcx, rbx
1804af4d8 mov      rbp, qword ptr [rsp + 0x40]
1804af4dd mov      rbx, qword ptr [rsp + 0x50]
1804af4e2 add      rsp, 0x30
1804af4e6 pop      rsi
1804af4e7 jmp      0x1804887f0 ; 
1804af4ec xor      edx, edx
1804af4ee mov      rcx, rbx
1804af4f1 mov      rbx, qword ptr [rsp + 0x50]
1804af4f6 add      rsp, 0x30
1804af4fa pop      rsi
1804af4fb jmp      0x180488060 ; 11113:MemoryPack.MemoryPackWriter.WriteNullObjectHeader
1804af500 call     0x180309d40 ; 

FUNCTION 9692 IotaInventory Deserialize 0x1804ae2f0 1008
1804ae2f0 mov      qword ptr [rsp + 0x18], rbx
1804ae2f5 push     rsi
1804ae2f6 push     rdi
1804ae2f7 push     r14
1804ae2f9 sub      rsp, 0x30
1804ae2fd cmp      byte ptr [rip + 0x2e8f36c], 0
1804ae304 mov      rdi, rdx
1804ae307 mov      rbx, rcx
1804ae30a jne      0x1804ae35b
1804ae30c lea      rcx, [rip + 0x2cf751d]
1804ae313 call     0x180309af0 ; 
1804ae318 lea      rcx, [rip + 0x2d0f8c1]
1804ae31f call     0x180309af0 ; 
1804ae324 lea      rcx, [rip + 0x2cdbaad]
1804ae32b call     0x180309af0 ; 
1804ae330 lea      rcx, [rip + 0x2cdc3e1]
1804ae337 call     0x180309af0 ; 
1804ae33c lea      rcx, [rip + 0x2cdf055]
1804ae343 call     0x180309af0 ; 
1804ae348 lea      rcx, [rip + 0x2cdf739]
1804ae34f call     0x180309af0 ; 
1804ae354 mov      byte ptr [rip + 0x2e8f315], 1
1804ae35b xor      r14d, r14d
1804ae35e cmp      byte ptr [rip + 0x2e8f292], r14b
1804ae365 mov      qword ptr [rsp + 0x50], r14
1804ae36a mov      qword ptr [rsp + 0x68], r14
1804ae36f jne      0x1804ae384
1804ae371 lea      rcx, [rip + 0x2cce698]
1804ae378 call     0x180309af0 ; 
1804ae37d mov      byte ptr [rip + 0x2e8f273], 1
1804ae384 cmp      dword ptr [rbx + 0x30], 1
1804ae388 jge      0x1804ae39c
1804ae38a xor      r8d, r8d
1804ae38d mov      edx, 1
1804ae392 mov      rcx, rbx
1804ae395 call     0x181705190 ; 11110:MemoryPack.MemoryPackReader.GetNextSpan
1804ae39a jmp      0x1804ae3b6 ; 
1804ae39c movups   xmm0, xmmword ptr [rbx + 0x20]
1804ae3a0 mov      rdx, qword ptr [rip + 0x2cce669]
1804ae3a7 lea      rcx, [rsp + 0x20]
1804ae3ac movaps   xmmword ptr [rsp + 0x20], xmm0
1804ae3b1 call     0x1804192f0 ; 201:System.DateTime.System.IConvertible.ToDateTime | 257:System.Int64.System.IConvertible.ToInt64 | 315:System.TimeSpan.get_Ticks | 334:System.UInt64.System.IConvertible.ToUInt64
1804ae3b6 movzx    esi, byte ptr [rax]
1804ae3b9 xor      r8d, r8d
1804ae3bc mov      edx, 1
1804ae3c1 mov      rcx, rbx
1804ae3c4 call     0x180486dd0 ; 
1804ae3c9 cmp      sil, 0xff
1804ae3cd je       0x1804ae5fc
1804ae3d3 cmp      sil, 2
1804ae3d7 je       0x1804ae4ad
1804ae3dd ja       0x1804ae471
1804ae3e3 cmp      qword ptr [rdi], r14
1804ae3e6 je       0x1804ae403
1804ae3e8 mov      rax, qword ptr [rdi]
1804ae3eb mov      rcx, qword ptr [rax + 0x10]
1804ae3ef mov      qword ptr [rsp + 0x50], rcx
1804ae3f4 test     rax, rax
1804ae3f7 je       0x1804ae6ce
1804ae3fd mov      rax, qword ptr [rax + 0x18]
1804ae401 jmp      0x1804ae40b ; 
1804ae403 mov      qword ptr [rsp + 0x50], r14
1804ae408 mov      rax, r14
1804ae40b mov      qword ptr [rsp + 0x68], rax
1804ae410 test     sil, sil
1804ae413 je       0x1804ae463
1804ae415 mov      qword ptr [rsp + 0x58], rbp
1804ae41a mov      rbp, qword ptr [rip + 0x2cdc2f7]
1804ae421 cmp      qword ptr [rbp + 0x38], r14
1804ae425 jne      0x1804ae42f
1804ae427 mov      rcx, rbp
1804ae42a call     0x18030dbb0 ; 
1804ae42f mov      r8, qword ptr [rbp + 0x38]
1804ae433 lea      rdx, [rsp + 0x50]
1804ae438 mov      rcx, rbx
1804ae43b mov      r8, qword ptr [r8 + 8]
1804ae43f call     0x180487bb0 ; 
1804ae444 mov      rbp, qword ptr [rsp + 0x58]
1804ae449 cmp      sil, 1
1804ae44d je       0x1804ae463
1804ae44f mov      r8, qword ptr [rip + 0x2cdf632]
1804ae456 lea      rdx, [rsp + 0x68]
1804ae45b mov      rcx, rbx
1804ae45e call     0x180487bb0 ; 
1804ae463 cmp      qword ptr [rdi], r14
1804ae466 je       0x1804ae57e
1804ae46c jmp      0x1804ae512 ; 
1804ae471 mov      rcx, qword ptr [rip + 0x2ea16e8]
1804ae478 mov      rbx, qword ptr [rip + 0x2cf73b1]
1804ae47f cmp      dword ptr [rcx + 0xe4], r14d
1804ae486 jne      0x1804ae48d
1804ae488 call     0x180309de0 ; 
1804ae48d xor      edx, edx
1804ae48f mov      rcx, rbx
1804ae492 call     0x18191dfa0 ; 328:System.Type.GetTypeFromHandle
1804ae497 mov      rcx, rax
1804ae49a xor      r9d, r9d
1804ae49d movzx    r8d, sil
1804ae4a1 mov      dl, 2
1804ae4a3 call     0x181706590 ; 11114:MemoryPack.MemoryPackSerializationException.ThrowInvalidPropertyCount
1804ae4a8 jmp      0x1804ae5ff ; 
1804ae4ad cmp      qword ptr [rdi], r14
1804ae4b0 je       0x1804ae541
1804ae4b6 mov      rax, qword ptr [rdi]
1804ae4b9 mov      rcx, qword ptr [rax + 0x10]
1804ae4bd mov      qword ptr [rsp + 0x50], rcx
1804ae4c2 test     rax, rax
1804ae4c5 je       0x1804ae6ce
1804ae4cb mov      rsi, qword ptr [rip + 0x2cdc246]
1804ae4d2 mov      rax, qword ptr [rax + 0x18]
1804ae4d6 mov      qword ptr [rsp + 0x68], rax
1804ae4db cmp      qword ptr [rsi + 0x38], r14
1804ae4df jne      0x1804ae4e9
1804ae4e1 mov      rcx, rsi
1804ae4e4 call     0x18030dbb0 ; 
1804ae4e9 mov      r8, qword ptr [rsi + 0x38]
1804ae4ed lea      rdx, [rsp + 0x50]
1804ae4f2 mov      rcx, rbx
1804ae4f5 mov      r8, qword ptr [r8 + 8]
1804ae4f9 call     0x180487bb0 ; 
1804ae4fe mov      r8, qword ptr [rip + 0x2cdf583]
1804ae505 lea      rdx, [rsp + 0x68]
1804ae50a mov      rcx, rbx
1804ae50d call     0x180487bb0 ; 
1804ae512 mov      rcx, qword ptr [rdi]
1804ae515 test     rcx, rcx
1804ae518 je       0x1804ae6ce
1804ae51e mov      rax, qword ptr [rsp + 0x50]
1804ae523 mov      qword ptr [rcx + 0x10], rax
1804ae527 mov      rcx, qword ptr [rdi]
1804ae52a test     rcx, rcx
1804ae52d je       0x1804ae6ce
1804ae533 mov      rax, qword ptr [rsp + 0x68]
1804ae538 mov      qword ptr [rcx + 0x18], rax
1804ae53c jmp      0x1804ae5ff ; 
1804ae541 mov      rsi, qword ptr [rip + 0x2cdb890]
1804ae548 cmp      qword ptr [rsi + 0x38], r14
1804ae54c jne      0x1804ae556
1804ae54e mov      rcx, rsi
1804ae551 call     0x18030dbb0 ; 
1804ae556 mov      rdx, qword ptr [rsi + 0x38]
1804ae55a mov      rcx, rbx
1804ae55d mov      rdx, qword ptr [rdx]
1804ae560 call     0x180487c70 ; 
1804ae565 mov      rdx, qword ptr [rip + 0x2cdee2c]
1804ae56c mov      rcx, rbx
1804ae56f mov      qword ptr [rsp + 0x50], rax
1804ae574 call     0x180487c70 ; 
1804ae579 mov      qword ptr [rsp + 0x68], rax
1804ae57e mov      rcx, qword ptr [rip + 0x2d0f65b]
1804ae585 call     0x180309ce0 ; 
1804ae58a cmp      byte ptr [rip + 0x2e8f0e0], r14b
1804ae591 mov      rsi, rax
1804ae594 jne      0x1804ae5b5
1804ae596 lea      rcx, [rip + 0x2d18aab]
1804ae59d call     0x180309af0 ; 
1804ae5a2 lea      rcx, [rip + 0x2ce0e37]
1804ae5a9 call     0x180309af0 ; 
1804ae5ae mov      byte ptr [rip + 0x2e8f0bc], 1
1804ae5b5 mov      rcx, qword ptr [rip + 0x2ce0e24]
1804ae5bc call     0x180309ce0 ; 
1804ae5c1 mov      rdx, qword ptr [rip + 0x2d18a80]
1804ae5c8 mov      rcx, rax
1804ae5cb mov      rbx, rax
1804ae5ce call     0x18162f090 ; 
1804ae5d3 xor      edx, edx
1804ae5d5 mov      qword ptr [rsi + 0x20], rbx
1804ae5d9 mov      rcx, rsi
1804ae5dc mov      byte ptr [rsi + 0x28], 1
1804ae5e0 call     0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing
1804ae5e5 mov      rax, qword ptr [rsp + 0x50]
1804ae5ea mov      qword ptr [rsi + 0x10], rax
1804ae5ee mov      rax, qword ptr [rsp + 0x68]
1804ae5f3 mov      qword ptr [rsi + 0x18], rax
1804ae5f7 mov      qword ptr [rdi], rsi
1804ae5fa jmp      0x1804ae5ff ; 
1804ae5fc mov      qword ptr [rdi], r14
1804ae5ff mov      rbx, qword ptr [rdi]
1804ae602 test     rbx, rbx
1804ae605 je       0x1804ae6c0
1804ae60b cmp      byte ptr [rip + 0x2e8f050], r14b
1804ae612 jne      0x1804ae64b
1804ae614 lea      rcx, [rip + 0x2d18ac5]
1804ae61b call     0x180309af0 ; 
1804ae620 lea      rcx, [rip + 0x2d18b51]
1804ae627 call     0x180309af0 ; 
1804ae62c lea      rcx, [rip + 0x2cc6cbd]
1804ae633 call     0x180309af0 ; 
1804ae638 lea      rcx, [rip + 0x2cc6d49]
1804ae63f call     0x180309af0 ; 
1804ae644 mov      byte ptr [rip + 0x2e8f017], 1
1804ae64b mov      rcx, qword ptr [rbx + 0x20]
1804ae64f test     rcx, rcx
1804ae652 je       0x1804ae6ce
1804ae654 mov      rdx, qword ptr [rip + 0x2d18b1d]
1804ae65b call     0x181604f10 ; 
1804ae660 mov      rax, qword ptr [rbx + 0x10]
1804ae664 mov      edi, r14d
1804ae667 mov      ecx, r14d
1804ae66a test     rax, rax
1804ae66d je       0x1804ae6ce
1804ae66f nop      
1804ae670 cmp      ecx, dword ptr [rax + 0x18]
1804ae673 jge      0x1804ae6c0
1804ae675 mov      rcx, qword ptr [rbx + 0x10]
1804ae679 test     rcx, rcx
1804ae67c je       0x1804ae6ce
1804ae67e mov      r8, qword ptr [rip + 0x2cc6c6b]
1804ae685 mov      edx, edi
1804ae687 mov      rsi, qword ptr [rbx + 0x20]
1804ae68b call     0x180fa5910 ; 
1804ae690 test     rsi, rsi
1804ae693 je       0x1804ae6ce
1804ae695 movups   xmm0, xmmword ptr [rax]
1804ae698 mov      r8, qword ptr [rip + 0x2d18a41]
1804ae69f lea      rdx, [rsp + 0x20]
1804ae6a4 mov      rcx, rsi
1804ae6a7 movaps   xmmword ptr [rsp + 0x20], xmm0
1804ae6ac call     0x181619910 ; 
1804ae6b1 mov      rax, qword ptr [rbx + 0x10]
1804ae6b5 inc      edi
1804ae6b7 mov      ecx, edi
1804ae6b9 test     rax, rax
1804ae6bc je       0x1804ae6ce
1804ae6be jmp      0x1804ae670 ; 
1804ae6c0 mov      rbx, qword ptr [rsp + 0x60]
1804ae6c5 add      rsp, 0x30
1804ae6c9 pop      r14
1804ae6cb pop      rdi
1804ae6cc pop      rsi
1804ae6cd ret      
1804ae6ce call     0x180309d40 ; 

FUNCTION 9692 IotaInventory .ctor 0x1804b04b0 112
1804b04b0 mov      qword ptr [rsp + 8], rbx
1804b04b5 push     rdi
1804b04b6 sub      rsp, 0x20
1804b04ba cmp      byte ptr [rip + 0x2e8d1b0], 0
1804b04c1 mov      rdi, rcx
1804b04c4 jne      0x1804b04e5
1804b04c6 lea      rcx, [rip + 0x2d16b7b]
1804b04cd call     0x180309af0 ; 
1804b04d2 lea      rcx, [rip + 0x2cdef07]
1804b04d9 call     0x180309af0 ; 
1804b04de mov      byte ptr [rip + 0x2e8d18c], 1
1804b04e5 mov      rcx, qword ptr [rip + 0x2cdeef4]
1804b04ec call     0x180309ce0 ; 
1804b04f1 mov      rdx, qword ptr [rip + 0x2d16b50]
1804b04f8 mov      rcx, rax
1804b04fb mov      rbx, rax
1804b04fe call     0x18162f090 ; 
1804b0503 xor      edx, edx
1804b0505 mov      qword ptr [rdi + 0x20], rbx
1804b0509 mov      rcx, rdi
1804b050c mov      byte ptr [rdi + 0x28], 1
1804b0510 mov      rbx, qword ptr [rsp + 0x30]
1804b0515 add      rsp, 0x20
1804b0519 pop      rdi
1804b051a jmp      0x18040c000 ; 67:Mono.Xml.SecurityParser.OnStartParsing | 67:Mono.Xml.SecurityParser.OnProcessingInstruction | 67:Mono.Xml.SecurityParser.OnIgnorableWhitespace | 67:Mono.Xml.SecurityParser.OnEndParsing

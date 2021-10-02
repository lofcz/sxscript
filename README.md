# SxScript

Embeddable scripting language for C# written in C#. Goals of this project are (in order of priority, desc):
- good test coverage
- support both adhoc evaluation and il bytecode
- high IO throughput via async/await
- safe by default, require explicit whitelisting of CLR interop
- allow limitation of execution time, memory used, intructions executed, recursion depth, enumerables length, iterations in looping statements
- syntactically be a relaxed subset of C# (inclined towards Lua, JS)
- visitable AST

# SxScript

Embeddable scripting language for C# written in C#.  

### Goals of this project are (in order of priority, desc)  
- good test coverage
- support both adhoc evaluation and il bytecode
- high IO throughput via async/await
- safe by default, require explicit whitelisting of CLR interop
- allow limitation of execution time, memory used, intructions executed, recursion depth, enumerables length, iterations in looping statements
- syntactically be a relaxed subset of C# (inclined towards Lua, JS)
- visitable AST

### Progress tracker  
- statements
  - branching
    - [x] if
    - [x] else if / else
    - [x] ternary (a ? b : c) 
    - [ ] switch
  - looping
    - [x] while
    - [x] for
    - [ ] foreach
  - jump
    - [x] goto
    - [ ] break
    - [ ] continue
    - [ ] return
   - labeled
    - [x] label
    - [ ] case
    - [ ] default
   - scope
    - [x] code block {}
    - [x] variable shadowing
   - logical 
    - [x] and / &&
    - [x] or / ||
    - [x] short circuit
   - bitwise
    - [ ] &
    - [ ] |
   - modifiers
    - [ ] async
    - [ ] public
    - [ ] private
    - [ ] static
   - base types
    - [ ] class
    - [x] null / nill 
    - [x] int
    - [x] double
    - [x] string
    - [x] bool
    - [x] object
    - [x] true, false   
 - operators
  - [x] assignment
  - [x] unary +, -
  - [x] binary +, -, *, /  
  - [ ] +=, *=, /=, -=
  - [ ] ++, --   
 - comments
  - [x] // single line
  - [ ] /* */ multiline comment. nesting won't be allowed
- evaluation
 - [x] interpretation
 - [ ] bytecode + vm
- CLR interop
 - [ ] TBD 
 
### Notes
- semicolons are optional
- parenthesis around keywords are optional. Both `if (condition)` and `if condition` are valid
- scopes are optional. Implicit scope is one statement. `a = 0 if 1 > 2 a = 2 print a` is valid
- tabs are always discarded (no ident / dedent)
- newlines are almost always discarded
- dynamic type control. Optional static types. Implicit var declaration. Both `a = 0` and `var a = 0` is valid. In future `int a = 0` should be valid too.

### Great projects I'd like to mention here
- https://github.com/codingseb/ExpressionEvaluator
- https://github.com/moonsharp-devs/moonsharp/
- https://github.com/scriban/scriban
- https://github.com/sebastienros/fluid
- https://github.com/sebastienros/jint
- https://github.com/microsoft/ClearScript
- https://github.com/munificent/craftinginterpreters (literature)


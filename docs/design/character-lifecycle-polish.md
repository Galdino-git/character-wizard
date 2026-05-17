# Design: Character Lifecycle Polish (F9)

## F9.1 — Feat picker modal

Cópia direta do padrão `ItemPickerModal`:
- Componente em `Components/Shared/FeatPickerModal.razor`
- `EventCallback<EntityRef> OnPick`
- `SearchService.Search(query, ["Feat"], …)` ranqueado por prefix

Integração em `LevelUpModal`: quando `_asiOrFeat == "feat"`, botão "🔍 Escolher feat" abre o modal. Resultado popula `_featName`/`_featSource` (mantendo o fallback de input manual).

## F9.2 — Spells on level-up

Em `LevelUpModal`, novo bloco condicional:

```csharp
if (cls.IsCaster())
{
    var oldLvl = Entry.Levels;
    var newLvl = oldLvl + 1;
    _cantripDelta = cls.CantripsKnownAtLevel(newLvl) - cls.CantripsKnownAtLevel(oldLvl);
    _spellDelta   = cls.SpellsKnownAtLevel(newLvl)   - cls.SpellsKnownAtLevel(oldLvl);
    // Show pickers if deltas > 0
}
```

UI:
- Lista colapsável "Cantrips novos: 0/N" e "Spells novos: 0/M" com selecionáveis filtrados via `SpellRepository.ForClassAtLevel`.
- Ao confirmar, `Character.KnownSpells.AddRange(_chosenSpells)`.

## F9.3 — Features gained

Helper novo em `ClassDataExtensions`:

```csharp
public static IEnumerable<ClassFeatureRef> FeaturesAtLevel(this ClassData cls, int level);
```

Parseia `classFeatures[]` (string e object com `classFeature` field), retorna apenas os com `Level == level`.

Análogo para subclass: `SubclassFeaturesAtLevel(SubclassData, int)`.

Em `LevelUpModal` renderiza:

```
Features ganhas neste nível:
• Extra Attack (Fighter nv 5)
• Style — Champion (Champion nv 3)
```

Clicáveis → `EntityDetailPanel category="classfeature"` (resolver retorna null no MVP, panel mostra "(sem descrição cadastrada)").

`SheetTab` ganha seção "Features de classe" listando todas as features até o nível atual.

## F9.4 — Multiclass

Mudanças no `LevelUpModal`:
- Topo do modal: radio "Subir <existing>" | "Adicionar classe (multiclass)".
- Modo multiclass: dropdown de classes (excluindo as já presentes), seleção cria novo `CharacterClassEntry { ClassRef, Levels = 0 }` que vira a entry corrente do modal.
- Subclass: se a classe nova tem `SubclassChoiceLevel == 1` (Cleric/Sorcerer/Warlock), o modal já força pick na mesma operação.
- HP: usa hit die da classe nova.
- Aviso visual (alert info): "Multiclass — pré-requisitos RAW (STR/DEX 13 etc.) não são verificados".

Após Apply:
- Se entrada é nova, adiciona a `Char.Classes`.
- Caso contrário, incrementa entry existente.

`CharacterView` SheetTab/header já junta `string.Join(" / ", c.Classes.Select(...))` — funciona.

## Tests

- `ClassDataExtensions.FeaturesAtLevel` — TDD: Fighter nv 5 contém "Extra Attack"; Wizard nv 2 contém "Arcane Tradition".
- `ClassDataExtensions.SpellsKnownDeltaAtLevel(old, new)` — opcional, basta o cálculo subtractive.
- `LevelUpRules` já testado.

## Decisões

- **Picker reusa SearchService** (zero infra nova).
- **Features gained não auto-aplicam efeitos** — são informativas. Aplicar Extra Attack como modificador seria escopo de "Combat Rules" (M4+).
- **Multiclass sem prereq check** — usuário adulto, ferramenta pessoal.
- **`SpellsKnownAtLevel` é cumulativo** (já está) — `delta = new - old` funciona direto.

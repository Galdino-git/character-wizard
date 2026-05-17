# Design: Wizard Enrichment (F4–F7)

Implementa REQ-W.1 a REQ-W.15 do [spec](../specs/wizard-enrichment.md).

## Estrutura de passos dinâmica

`NewCharacter.razor` hoje tem array fixo de strings `_steps`. Refatorar para gerar dinamicamente baseado em `Draft`:

```csharp
private List<WizardStep> BuildSteps()
{
    var s = new List<WizardStep>
    {
        new("Origem", () => <BasicsStep />),
        new("Raça",   () => <RaceSelectStep />),
    };
    if (Draft.RaceRef is { } r && RaceRepo.SubracesOf(r).Any())
        s.Add(new("Subraça", () => <SubraceSelectStep />));
    s.Add(new("Classe", () => <ClassSelectStep />));
    if (Draft.ClassRef is { } c && SubclassLevel(c) is int lvl && Draft.InitialLevel >= lvl)
        s.Add(new("Subclass", () => <SubclassSelectStep />));
    s.Add(new("Background", () => <BackgroundSelectStep />));
    s.Add(new("Atributos", () => <AbilitiesStep />));
    if (Draft.ClassRef is { } cc && IsCaster(cc))
        s.Add(new("Spells", () => <SpellSelectStep />));
    s.Add(new("Equipamento", () => <EquipmentStep />));
    s.Add(new("Revisão", () => <ReviewStep />));
    return s;
}
```

Razor não permite delegate retornando `RenderFragment` inline trivialmente — alternativa: switch baseado em `step.Key`. Implementação real usa switch por string Key.

## Componentes novos

- `Components/Wizard/SubraceSelectStep.razor` (F4)
- `Components/Wizard/SubclassSelectStep.razor` (F5)
- `Components/Wizard/SpellSelectStep.razor` (F6)
- `Components/Wizard/EquipmentStep.razor` (F7)

Cada um lê/escreve no `CharacterDraft` injetado.

## Extensões em CharacterDraft

```csharp
public string? SubraceName { get; set; }
public EntityRef? SubclassRef { get; set; }
public List<EntityRef> KnownSpells { get; set; } = new();
public List<InventoryItem> Inventory { get; set; } = new();
```

`ToCharacter()` propaga todos.

## Helpers em Data

- `ClassData.SubclassChoiceLevel()` — extension method que parseia `classFeatures` procurando o item com `gainSubclassFeature: true` e retorna o `Level`.
- `SpellRepository.ForClassAtLevel(className, classSource, spellLevel)` — filtra spells cujo `classes.fromClassList[]` inclui a classe e que sejam do `spellLevel` pedido.

## Tests (TDD onde adiciona valor)

- `ClassData_SubclassChoiceLevel_returns_correct_level` (Wizard=2, Cleric=1, Fighter=3, Warlock=1, Sorcerer=1)
- `SpellRepository_ForClassAtLevel_filters_correctly` (carrega catálogo real, valida que `Fireball` aparece para `Wizard` nível 3 mas não nível 1)
- `CharacterDraft.ToCharacter_propagates_subrace_subclass_spells_inventory`

## Decisões

- **Passos dinâmicos**: o usuário pode voltar para Class e mudar — recomputar steps a cada `Navigate`/`Pick`. Estado de outros passos é preservado (Draft é a fonte).
- **Parse heterogêneo de startingEquipment**: usar fallback texto + checkbox no MVP. Implementar parser estruturado depois.
- **Spell filtering**: 5etools usa `spell.classes.fromClassList[]` com `{name, source}`. Match case-insensitive em ambos os campos.
- **Subclass progression**: features de subclass não são auto-aplicadas (apenas referenciadas via `SubclassRef`) — Ficha mostra "Subclass: X" e o `EntityDetailPanel` é o lugar de ler features.

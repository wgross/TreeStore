# TreeStore
A powershell file system provide for storing simply structured data

## Installation

None yet. PSGallery support is coming.

## Create a TreeStore file system.

Create non-persistent in memory drive:

```powershell
Import-Module TreeStore
New-PSDrive -Name tree -PsProvider TreeStore -Root ""
cd tree:
```

## Create an Entity

An entity is the main item of a TreeStore file system. Its Name is unique in its folder like a file name. All entites are stored under /Entities folder.
```powershell
NewItem \Entities\e
```
To attach additional data to an entity you need to define the data strcuture first. Data Structures are called 'Tag' in TreeStore.

## Create a Tag

All Tags are find in /Tags directory Tag is a simple data structure. Its name must be unique.
```powershell
New-Item /Tags/t 
```
A Tag may define properties with name and type to later data storage. These are called facets or facet properties.

## Create a Facet Property

To add properties definition to a tag just create a new item under the tag.
The name must be unique within the Tag.
```powershell
New-Item /Tags/t -Name p -ValueType Long
```
Supported property types are: Bool, DateTime, Decimal, Double, Guid, Long, String.

## Assign Tag to an Entity

To assign a Tag 't' to En ecisting entity 'e' just create a new item named like a tag under the entity:
```powershell
New-Item /Entities/e -Name t
```
Now properties define by the Tag can be filled with values. If the name of the tag is't found in the /Tags folder the operation will fail.

## Assign a value to a Entites' Facet Property

A value can be assigned with the Set-ItemProperty cmdlet:
```powershell
Set-ItemProperty -Path /Entites/e -Name p -Value 1
```

Other Powershell Provider Cmdlet work analogous to their impolememtaion of a normal file system.


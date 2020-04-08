# TreeStore
A powershell file system for storing simple structured data. Implementation of the file system provider relies heavily on an extended fork of [beefarinos p2f framework](https://github.com/beefarino/p2f). The fork can be found [here](https://github.com/wgross/p2f). 

## Installation
Install from PSGallery:
```powershell
Install-Module TreeStore -AllowPrerelease
```
The module project targets netstandard 2.0 and is therefore compatible with PowerShell 5 and PowerShell Core.

## Create a TreeStore file system.
TreeStore provides its own Ccmdlet to create PSDrives for convenience. You can achive the same with New-PsDrive but filling the cmdlet-arguments is more straightforward using the custom cmdlet.

Create non-persistent (in memory) drive:
```powershell
Import-Module TreeStore
New-TreeStoreDrive -Name tree
cd tree:
```
Create a persistent drive based on a LiteDb database.
```powershell
Import-Module TreeStore
New-TreeStoreDrive -Name tree -TreeStorePath "c:\tmp\data.db"
cd tree:
```
[more about TreeStore drives...](https://github.com/wgross/TreeStore/wiki/New-TreeStoreDrive)

## Create a Tag
A Tag defines a simple data structure composed of uniquely named properties. The name of the tag is also unique. To create Tag just create a new item in directory /Tags.
```powershell
New-Item \Tags\example_tag
```
### Create a Facet Property
To add facet properties to a tag just create a new item under the tag.
The name must be unique within the Tag. For any property a data type has to be provided.
```powershell
New-Item \Tags\example_tag -Name example_property -ValueType Long
```
Supported property types are: Bool, DateTime, Decimal, Double, Guid, Long, String. Properties can be renamed or copyed/moved to another tag or removed from a tag with approriate powershell item cmdlets. Since facet properties can exist only with in a tag destinations outside of a tag are not allowed for copying or moving

[more about Tags and Facet Properties...](https://github.com/wgross/TreeStore/wiki/Tags)

## Create an Entity
An entity is the main item of a TreeStore file system. All entities are stored under the \Entities folder.
```powershell
NewItem \Entities\example_entity
```
[more about Entities...](https://github.com/wgross/TreeStore/wiki/Entities)

## Assign Tag to an Entity
To assign a Tag 't' to an existing entity 'e' just create a new item named like a tag under the entity:
```powershell
New-Item \Entities\example_entity -Name example_tag
```
Now properties defined by the Tag can be filled with values. If the name of the tag isn't found in the \Tags folder the operation will fail.

## Set/Get value of an Entities Facet Property
A value can be assigned with the Set-ItemProperty cmdlet:
```powershell
Set-ItemProperty -Path \Entities\example_entity -Name example_property -Value 1
```
The value can be read again using:
```powershell
Get-ItemPropertyValue -Path \Entities\example_entity -Name example_property

1
```



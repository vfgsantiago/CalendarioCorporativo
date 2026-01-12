# 🗓️ Calendário Corporativo

![Badge Status](https://img.shields.io/badge/Status-Concluido-brightgreen)
![Badge .NET](https://img.shields.io/badge/Backend-.NET%20Core-purple)
![Badge Feature](https://img.shields.io/badge/Integration-.ICS%20%2F%20Outlook-blue)

> **Sincronia total entre setores. Visibilidade para toda a empresa.**

Uma solução de calendário corporativo que permite a divulgação descentralizada de eventos (cada setor gerencia o seu), mas oferece uma visualização centralizada e unificada para o colaborador, com integração nativa ao Outlook/Teams.

---

## 🎯 Visão Geral
O sistema resolve o problema de comunicação fragmentada de eventos. Ele permite que o RH, o Marketing e a TI agendem seus próprios eventos de forma independente, enquanto o colaborador final acessa um portal único, filtra o que lhe interessa e sincroniza com sua agenda pessoal.

---

## ✨ Funcionalidades Principais

### 1. 🌍 Portal Público (Visualização)
Onde a mágica acontece para o usuário final.
* **Visualização Interativa:** Calendário dinâmico com visões Mensal, Semanal e Diária.
* **Filtros Avançados:**
    * **Por Setor:** "Quero ver apenas eventos do RH".
    * **Por Categoria:** "Mostrar apenas Treinamentos" ou "Apenas Feriados".
* **Badges Visuais:** Cada evento exibe a cor e o ícone definidos na sua categoria (ex: 🎓 para Treinamentos, 🎉 para Confraternizações).
* **Exportação .ICS (Outlook/Teams):** Botão "Adicionar à minha agenda" em cada evento, gerando um arquivo compatível com Outlook, Google Calendar e MacOS Calendar.

### 2. 🛡️ Painel Administrativo (Gestão)
Back-office com controle de acesso rigoroso.
* **Isolamento de Dados por Setor:** O usuário do "Setor Financeiro" **não vê nem edita** eventos do "Setor de TI". Cada departamento tem autonomia e privacidade sobre sua gestão.
* **Gestão de Categorias Personalizadas:**
    * Definição de **Nome** (ex: Workshop).
    * Escolha de **Cor** (para a badge).
    * Seleção de **Ícone**.
* **Gestão de Eventos:** Cadastro completo com Data, Hora, Local, Link (Teams/Zoom) e Descrição Rica.

---

## 🛠️ Tecnologias Utilizadas

* **Linguagem:** C# (.NET)
* **Backend/Frontend:** ASP.NET Core (MVC & Web API)
* **Banco de Dados:** Oracle PLSQL
* **Estilização:** Bootstrap / CSS3 / AJAX / JQUERY

---

## 🛠️ Metodoloias Utilizadas

* **Arquitetura:** Camadas
* **Padrão:** Repository Pattern
  
---

## 🔄 Fluxo da Aplicação

```mermaid
graph TD
    subgraph "Área Administrativa"
    AdminTI[Admin TI] -->|Cria| EventoA[Manutenção Servidores]
    AdminRH[Admin RH] -->|Cria| EventoB[Treinamento Compliance]
    AdminRH -->|Não Vê| EventoA
    end

    subgraph "Portal Público"
    User((Colaborador)) -->|Acessa| Cal[Calendário Unificado]
    EventoA --> Cal
    EventoB --> Cal
    
    User -->|Filtra| Filter{Filtros}
    Filter -->|Categoria| View1[Ver apenas Treinamentos]
    Filter -->|Setor| View2[Ver apenas TI]
    
    User -->|Clica| Detail[Detalhes do Evento]
    Detail -->|Download| ICS[.ICS File]
    ICS -->|Importa| Outlook[Outlook / Teams]
    end

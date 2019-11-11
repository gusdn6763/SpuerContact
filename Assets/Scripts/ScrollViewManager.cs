﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//public class ContactComprer : IComparer<Contact1>
//{
//    public int Compare(Contact1 x, Contact1 y)
//    {
//        return x.name.CompareTo(y.name);
//    }
//}

public class ScrollViewManager : ViewManager, ICell
{
    [SerializeField] GameObject cellPrefab;
    [SerializeField] GameObject addPopupViewPrefab;
    [SerializeField] GameObject detailViewPrefab;
    [SerializeField] GameObject confirmPopupViewPrefab;

    [SerializeField] RectTransform content;

    List<Cell> cellList = new List<Cell>();

    float cellHeight = 240f;
    Contacts? contacts;
    
    // Cell 편집 버튼 관련 변수
    bool isEditable = false;

    private void Awake() 
    {
        // Title 지정
        title = "SuperContact";

        // Add 버튼 지정
        rightNavgationViewButton = Instantiate(buttonPrefab).GetComponent<SCButton>();
        rightNavgationViewButton.SetTitle("Add");
        rightNavgationViewButton.SetOnClickAction(() =>
        {
            // AddPopupViewManager를 표시하는 동작 구현
            AddPopupViewManager addPopupViewManager = Instantiate(addPopupViewPrefab, mainManager.transform).GetComponent<AddPopupViewManager>();
            // 새로운 연락처를 추가했을때 할 일
            addPopupViewManager.addContactCallback = (contact) =>
            {
                AddContact(contact);
                AddCell(contact, contacts.Value.contactList.Count - 1);
            };

            // AddPopupViewManager 열기
            addPopupViewManager.Open();
        });

        // 왼쪽 버튼 (Edit: 셀을 삭제할 수 있는 기능)
        leftNavgationViewButton = Instantiate(buttonPrefab).GetComponent<SCButton>();
        leftNavgationViewButton.SetTitle((isEditable) ? "Complete" : "Delete");
        leftNavgationViewButton.SetOnClickAction(() =>
        {
            isEditable = !isEditable;

            if (isEditable)
            {
                leftNavgationViewButton.SetTitle("Complete");
                foreach (Cell cell in cellList)
                {
                    cell.ActiveDelete = true;
                }
            }
            else
            {
                leftNavgationViewButton.SetTitle("Delete");
                foreach (Cell cell in cellList)
                {
                    cell.ActiveDelete = false;
                }
            }
        });

    }

    private void Start() 
    {
        contacts = FileManager<Contacts>.Load(Constant.kFileName);
        LoadData();
    }

    // TODO: Contacts에 있는 정보를 Cell로 만들어서 추가
    void LoadData()
    {
        if (contacts.HasValue)
        {          
            Contacts contactsValue = contacts.Value;
            contactsValue.contactList.Sort();
            for (int i = 0; i < contactsValue.contactList.Count; i++)
            {
                AddCell(contactsValue.contactList[i], i);
            }
        }
    }

    // Contact 정보로 Cell 객체를 만들어서 content에 추가하는 함수
    void AddCell(Contact contact, int index)
    {       
        Cell cell = Instantiate(cellPrefab, content).GetComponent<Cell>();
        cell.Name = contact.name;
        cell.PhoneNuber = contact.phoneNumber;
        cell.Email = contact.email;
        cell.Icon.sprite = contact.sp;

        cell.cellDelegate = this;
        cellList.Add(cell);
        // Content의 높이 재조정
        AdjustContent();

        cellList.Sort();

        Contacts contactsValue = contacts.Value;
        contactsValue.contactList.Sort();
    }

    // Contacts에 Contact 추가
    void AddContact(Contact contact)
    {
        if (contacts.HasValue)
        {
            Contacts contactsValue = contacts.Value;
            contactsValue.contactList.Add(contact);
        }
        else
        {
            List<Contact> contactsList = new List<Contact>();
            contactsList.Add(contact);

            contacts = new Contacts(contactsList);
        }
    }

    // Content의 높이 재조정
    void AdjustContent()
    {
        if (contacts.HasValue)
        {
            Contacts contactsValue = contacts.Value;
            content.sizeDelta = new Vector2(0, contactsValue.contactList.Count * cellHeight);
        }
        else
        {
            content.sizeDelta = Vector2.zero;
        }
    }

    void OnApplicationQuit()
    {
        if (contacts.HasValue)
            FileManager<Contacts>.Save(contacts.Value, Constant.kFileName);
    }

    // Cell이 터치 되었을때 호출하는 함수
    public void DidSelectCell(Cell cell)
    {
        if (contacts.HasValue)
        {
            int cellIndex = cellList.IndexOf(cell);

            DetailViewManager detailViewManager
                = Instantiate(detailViewPrefab).GetComponent<DetailViewManager>();

            Contact selectedContact = contacts.Value.contactList[cellIndex];
            detailViewManager.contact = selectedContact;

            detailViewManager.saveDelegate = (newContact) =>
            {
                contacts.Value.contactList[cellIndex] = newContact;
                
                cell.Name = newContact.name;
                cell.PhoneNuber = newContact.phoneNumber;
                cell.Email = newContact.email;
                cell.Icon.sprite = newContact.sp;
                //ClearCell();
                //LoadData();
            };

            mainManager.PresentViewManager(detailViewManager);
        }
    }

    void ClearCell()
    {
        foreach (Cell cell in cellList)
        {
            Destroy(cell.gameObject);
        }
        cellList.RemoveRange(0, cellList.Count);
    }

    public void DidSelectDeleteCell(Cell cell)
    {
        if (contacts.HasValue)
        {
            ConfirmPopupViewManager confirmPopupViewManager 
                = Instantiate(confirmPopupViewPrefab, mainManager.transform)
                    .GetComponent<ConfirmPopupViewManager>();

            confirmPopupViewManager.confirmPopupViewManagerDelegate = () => {
                int cellIndex = cellList.IndexOf(cell);
                List<Contact> contactList = contacts.Value.contactList;
                contactList.RemoveAt(cellIndex);
                cellList.RemoveAt(cellIndex);
                Destroy(cell.gameObject);
                AdjustContent();
            };
            
            confirmPopupViewManager.Open();
        }
    }
}

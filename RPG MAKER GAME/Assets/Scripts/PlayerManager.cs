﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MovingObject
{
    public string currentMapName; //transferMap 스크립트에 있는 transferMapName 변수의 값을 저장.

    public static MovingObject instance;
    //static : 정적변수로서, 해당 스크립트가 적용된 모든 객체들은 static으로 선언된 변수의 값을 공유함.

    public string walkSound_1;
    public string walkSound_2;
    public string walkSound_3;
    public string walkSound_4;

    private AudioManager theAudio;

    public float runSpeed;
    private float applyRunSpeed;
    private bool applyRunFlag = false;
    private bool canMove = true;

    void Start()
    {
        //DontDestoryOnLoad 때문에 Player가 여러 개 생기는 현상 방지.
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            boxCollider = GetComponent<BoxCollider2D>();
            animator = GetComponent<Animator>();
            theAudio = FindObjectOfType<AudioManager>();
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        //처음 생성된 경우에만 instance 값이 null.
        //왜냐하면 생성된 이후에 this 값을 주었기 때문.
        //그리고 나서, 해당 스크립트가 적용된 객체가 또 생성될 경우,
        //static으로 값을 공유한 instance의 값이 this이기 때문에 그 객체는 삭제됨.
    }

    /*
    private void Awake()
    {
        if (instance == null)
        {
            DontDestroyOnLoad(this.gameObject);
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }*/

    IEnumerator MoveCoroutine() //한 번에 여러 입력X, 텀을 주기 위해서. 다중처리함수처럼 작동.
    {
        //걷고 coroutine으로 끊고를 반복하다보니 한 발로 걷는 것처럼 보이는 문제를 해결하기 위해 while문 추가.
        //걷고 coroutine으로 끊고 반복이 아니고, coroutine은 update에서 한 번만 실행되고 나머지 입력은 coroutine 내부의 while문에서 처리됨.
        while (Input.GetAxisRaw("Vertical") != 0 || Input.GetAxisRaw("Horizontal") != 0)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                applyRunSpeed = runSpeed; //shift 누르면 더 빠르게 이동
                applyRunFlag = true;
            }
            else
            {
                applyRunSpeed = 0;
                applyRunFlag = false;
            }

            vector.Set(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), transform.position.z);
            //vector.x = Input.GetAxisRaw("Horizontal") 값.
            //Input.GetAxisRaw("Horizontal") : 우 방향키가 눌리면 1 리턴, 좌 방향키가 눌리면 -1 리턴
            //상하도 위와 동일.

            if (vector.x != 0) //방향키를 상하+좌우 동시에 누른 경우(대각선), 좌우로 이동하는 경우에는
                vector.y = 0; //무조건 좌우 방향만 보도록. 몸이 상하로 틀어지지 않게.


            animator.SetFloat("DirX", vector.x);
            animator.SetFloat("DirY", vector.y);

            bool checkCollisionFlag = base.CheckCollision();
            if (checkCollisionFlag) //앞에 뭐가 있으면
                break; //밑에 문장 실행X

            animator.SetBool("Walking", true); //걷는 모션으로 바꾸기

            //걷는 사운드

            int temp = Random.Range(1, 4); //걷는 사운드 랜덤 선택
            switch (temp)
            {
                case 1:
                    theAudio.Play(walkSound_1);
                    break;
                case 2:
                    theAudio.Play(walkSound_2);
                    break;
                case 3:
                    theAudio.Play(walkSound_3);
                    break;
                case 4:
                    theAudio.Play(walkSound_4);
                    break;
            }
            theAudio.SetVolumn(walkSound_2, 0.5f); //사운드 크기 반으로 줄이기

            //실제 이동이 이루어지는 부분
            while (currentWalkCount < walkCount)
            {
                if (vector.x != 0)
                {
                    transform.Translate(vector.x * (speed + applyRunSpeed), 0, 0);
                }
                else if (vector.y != 0)
                {
                    transform.Translate(0, vector.y * (speed + applyRunSpeed), 0);
                }
                if (applyRunFlag) //shift 눌렀을 땐 두 칸씩 이동하도록 하기 위해 ++ 두 번 되게!
                {
                    currentWalkCount++;
                }
                currentWalkCount++;

                yield return new WaitForSeconds(0.01f);
            }
            currentWalkCount = 0;
        }
        //while문 밖
        animator.SetBool("Walking", false); //서 있는 모션으로 바꾸기
        canMove = true;
    }


    // Update is called once per frame
    void Update()
    {
        if (canMove) //MoveCoroutine 함수가 여러 개 동시에 실행되는 것 방지
        {
            if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
            {
                canMove = false;
                StartCoroutine(MoveCoroutine());
            }
        }
    }
}

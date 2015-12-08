using UnityEngine;

public class Step<T> where T : struct
{
    protected T previous;
    protected T current;
    protected T next;
    protected T none;

    protected float time;           // STEP이 변하고 난 후의 경과 시간
    protected float previous_time;  // 직전에 do_execution() 했을 때의 time
    protected int count;

    protected Status status;
    protected struct Status
    {
        public bool is_changed;
    };

    protected Delay delay;
    protected struct Delay
    {
        public float delay;
        public T next;
    };


    public Step(T none)
    {
        this.none = none;
        init();
    }

    public void init()
    {
        previous = none;
        current = none;
        next = none;

        previous_time = -1.0f;
        time = 0.0f;
        count = 0;

        status.is_changed = false;

        delay.delay = -1.0f;
        delay.next = none;        
    }

    public void release(){ init(); }

    // 다음 스텝 설정
    public void set_next(T step){ next = step; }

    // 다음 스텝 호출
    public T get_next(){ return next; }

    // delay[sec] 기다렸다가 다음 스텝으로 전환한다.
    public void set_next_delay(T step, float delay)
    {
        next = none;

        this.delay.delay = delay;
        this.delay.next = step;
    }

    // 현재 스텝을 가져온다
    public T get_current() { return current; }

    // 이전 스텝을 가져온다
    public T get_previous() { return previous; }

    // 스텝이 전환된 순간
    public bool is_changed() { return status.is_changed; }

    // 스텝 내 경과시간을 가져온다
    public float get_time() { return time; }
    
    // 직전 실행 시 경과 시간을 가져온다
    public float get_previous_time() { return previous_time; }

    //==================================================================

    // 시작 처리
    public T do_initialize()
    {
        T step;

        if (!next.Equals(none))
        {
            step = next;

            previous = current;
            current = next;
            next = none;
            time = -1.0f;
            count = 0;

            status.is_changed = true;
        }
        else
        {
            //전환이 일어나지 않음
            step = none;
        }
        return step;
    }


    // 상태 전환 처리 
    public T do_transition()
    {
        T step;
        if (!delay.next.Equals(none))
        {
            step = none;

            if(delay.delay <= 0.0f)
            {
                next = delay.next;
                delay.delay = -1.0f;
                delay.next = none;
            }
        }
        else
        {
            if (next.Equals(none))
            {
                step = current;
            }
            else
            {
                // 전환이 결정된 경우는 하지 않는다
                step = none;
            }
        }
        return step;
    }

    // 실행 처리 
    public T do_execution(float passage_time)
    {
        T step;
        if (delay.delay >= 0.0f)
        {
            delay.delay -= passage_time;
            step = none;
        }
        else
        {
            if (!current.Equals(none))
            {
                step = current;
            }
            else
            {
                step = none;
            }

            count++;
            previous_time = time;

            if (time < 0.0f)
            {
                time = 0.0f;
            }
            else
            {
                time += passage_time;
            }

        }
        return step;
    }

    // 일시정지
    public void sleep() { current = none; }

    // time 교차 지점
    public bool is_acrossiong_time(float time)
    {
        return (previous_time < time && time <= this.time);
    }

    public bool is_acrossiong_cycle(float cycle)
    {
        return (Mathf.Ceil(previous_time/ cycle)<Mathf.Ceil(time/ cycle));
    }
}
